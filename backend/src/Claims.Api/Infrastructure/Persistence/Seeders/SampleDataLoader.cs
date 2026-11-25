using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Claims.Api.Domain;

namespace Claims.Api.Infrastructure.Persistence.Seeders;

/// <summary>The users and claims built from the bundled synthetic data set.</summary>
public sealed record SampleData(IReadOnlyList<User> Users, IReadOnlyList<Claim> Claims);

/// <summary>
/// Maps the synthetic APAC data set (CSV files embedded under <c>SampleData/</c>) onto the current domain model.
/// ADR-0021 records the mapping: ten source statuses collapse into five, amounts convert to USD, and policies,
/// reserves, payments, documents and notes are not loaded because the model has nowhere to put them.
/// </summary>
public static class SampleDataLoader
{
    public const string ResourcePrefix = "SampleData/";

    private const string WithdrawnNote = "Withdrawn by the claimant";
    private const int MaxText = 2000;

    public static SampleData Load() => Map(
        ReadResource("markets.csv"),
        ReadResource("claims.csv"),
        ReadResource("claim_status_history.csv"),
        ReadResource("claim_info_requests.csv"));

    public static SampleData Map(
        string marketsCsv,
        string claimsCsv,
        string historyCsv,
        string infoRequestsCsv)
    {
        var fxToUsd = CsvReader.Read(marketsCsv).ToDictionary(
            r => r["market_code"],
            r => decimal.Parse(r["fx_rate_to_usd"], NumberStyles.Float, CultureInfo.InvariantCulture));

        // The source has 900 customers and 68 officers. Acting as cannot list them, so every sample person is one of
        // two users: SampleClaimant, or Sample Staff for anyone who handled a claim, including a rejection.
        var users = new List<User>
        {
            new(DemoUsers.SampleClaimant, "SampleClaimant", UserRole.Claimant),
            new(DemoUsers.SampleStaff, "Sample Staff", UserRole.Officer, DemoUsers.SampleTeam),
        };

        var historyByClaim = CsvReader.Read(historyCsv)
            .GroupBy(r => r["claim_id"])
            .ToDictionary(g => g.Key, g => g.OrderBy(r => ParseTime(r["occurred_at"])).ToList());
        var requestsByClaim = CsvReader.Read(infoRequestsCsv)
            .GroupBy(r => r["claim_id"])
            .ToDictionary(g => g.Key, g => g.ToList());

        var claims = CsvReader.Read(claimsCsv)
            .Select(r => MapClaim(
                r,
                fxToUsd[r["market_code"]],
                historyByClaim.GetValueOrDefault(r["claim_id"]) ?? [],
                requestsByClaim.GetValueOrDefault(r["claim_id"]) ?? []))
            .ToList();

        return new SampleData(users, claims);
    }

    /// <summary>The ten source statuses collapse into the five the API knows (ADR-0021).</summary>
    public static ClaimStatus MapStatus(string sourceStatus) => sourceStatus switch
    {
        "SUBMITTED" => ClaimStatus.Submitted,
        "ASSIGNED" or "UNDER_REVIEW" or "REOPENED" => ClaimStatus.UnderReview,
        "AWAITING_INFO" => ClaimStatus.InfoRequested,
        "APPROVED" or "SETTLED" or "CLOSED" => ClaimStatus.Settled,
        "REJECTED" or "WITHDRAWN" => ClaimStatus.Rejected,
        _ => throw new InvalidDataException($"Unknown sample claim status '{sourceStatus}'."),
    };

    /// <summary>A stable id, so loading the same row twice finds the same entity.</summary>
    public static Guid IdFor(string sourceId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"claims-sample-data/{sourceId}"));
        var bytes = hash.AsSpan(0, 16).ToArray();
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x50); // version 5 layout
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80); // RFC 4122 variant
        return new Guid(bytes, bigEndian: true);
    }

    private static Claim MapClaim(
        Dictionary<string, string> row,
        decimal fxToUsd,
        List<Dictionary<string, string>> history,
        List<Dictionary<string, string>> requests)
    {
        var claimId = IdFor(row["claim_id"]);
        var claimantId = DemoUsers.SampleClaimant;
        var assigneeId = row["assigned_staff_id"].Length > 0 ? DemoUsers.SampleStaff : (Guid?)null;
        var status = MapStatus(row["status"]);

        // System events (the payments service) have no user row, so they are attributed to the claim's officer.
        var fallbackActor = assigneeId ?? claimantId;
        var changes = MapHistory(claimId, history, fallbackActor);

        var decided = status is ClaimStatus.Settled or ClaimStatus.Rejected;
        var decidedAt = decided
            ? (row["decided_at"] is { Length: > 0 } at ? ParseTime(at) : changes[^1].ChangedAt)
            : (DateTimeOffset?)null;

        return Claim.Restore(
            claimId,
            row["claim_number"],
            claimantId,
            assigneeId,
            Enum.Parse<ClaimType>(row["line_of_business"], ignoreCase: true),
            status,
            DateOnly.FromDateTime(ParseTime(row["incident_at"]).UtcDateTime),
            Truncate(row["description"]),
            decimal.Round(decimal.Parse(row["estimated_loss"], CultureInfo.InvariantCulture) * fxToUsd, 2),
            decided ? (status == ClaimStatus.Settled ? DecisionOutcome.Settled : DecisionOutcome.Rejected) : null,
            decided ? RejectionReason(row, status) : null,
            decidedAt,
            ParseTime(row["reported_at"]),
            changes,
            requests.Select(MapRequest));
    }

    /// <summary>
    /// Replays the source events as status changes in the five-status model. An event that leaves the mapped status
    /// unchanged (assigned then under review, approved then settled then closed) adds no row.
    /// </summary>
    private static List<ClaimStatusChange> MapHistory(Guid claimId, List<Dictionary<string, string>> events, Guid fallbackActor)
    {
        var changes = new List<ClaimStatusChange>();
        foreach (var e in events)
        {
            var to = MapStatus(e["to_status"]);
            ClaimStatus? from = changes.Count == 0 ? null : changes[^1].ToStatus;
            if (from == to)
            {
                continue;
            }

            var actor = e["actor_id"].StartsWith("CUS-", StringComparison.Ordinal)
                ? DemoUsers.SampleClaimant
                : e["actor_id"].StartsWith("STF-", StringComparison.Ordinal) ? DemoUsers.SampleStaff : fallbackActor;
            var note = e["to_status"] == "WITHDRAWN" ? WithdrawnNote : NullIfEmpty(e["note"]);
            changes.Add(new ClaimStatusChange(Guid.Parse(e["event_id"]), claimId, from, to, actor, ParseTime(e["occurred_at"]), note));
        }

        return changes;
    }

    private static InformationRequest MapRequest(Dictionary<string, string> row)
    {
        var answered = row["status"] == "RESPONDED" && row["responded_at"].Length > 0;
        return new InformationRequest(
            Guid.Parse(row["info_request_id"]),
            IdFor(row["claim_id"]),
            DemoUsers.SampleStaff,
            Truncate(row["message"].Length > 0 ? row["message"] : row["requested_items"]),
            ParseTime(row["requested_at"]),
            answered ? NullIfEmpty(row["response_message"]) : null,
            answered ? ParseTime(row["responded_at"]) : null);
    }

    private static string? RejectionReason(Dictionary<string, string> row, ClaimStatus status)
    {
        if (status != ClaimStatus.Rejected)
        {
            return null;
        }

        if (row["status"] == "WITHDRAWN")
        {
            return WithdrawnNote;
        }

        // BELOW_DEDUCTIBLE becomes "Below deductible".
        var code = row["decision_reason_code"].Replace('_', ' ').ToLowerInvariant();
        return code.Length == 0 ? "Rejected" : char.ToUpperInvariant(code[0]) + code[1..];
    }

    private static string ReadResource(string fileName)
    {
        var assembly = typeof(SampleDataLoader).Assembly;
        using var stream = assembly.GetManifestResourceStream(ResourcePrefix + fileName)
            ?? throw new FileNotFoundException($"Embedded sample data '{fileName}' not found.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static DateTimeOffset ParseTime(string value) =>
        DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    private static string? NullIfEmpty(string value) => value.Length == 0 ? null : value;

    private static string Truncate(string value) => value.Length <= MaxText ? value : value[..MaxText];
}

/// <summary>A small RFC 4180 reader: quoted fields, doubled quotes, commas and line breaks inside quotes.</summary>
internal static class CsvReader
{
    public static List<Dictionary<string, string>> Read(string text)
    {
        var rows = ParseRows(text.TrimStart('﻿'));
        var header = rows[0];
        return [.. rows.Skip(1).Select(r => header.Zip(r, (h, v) => (h, v)).ToDictionary(p => p.h, p => p.v))];
    }

    private static List<List<string>> ParseRows(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    quoted = false;
                }
                else
                {
                    field.Append(c);
                }

                continue;
            }

            switch (c)
            {
                case '"':
                    quoted = true;
                    break;
                case ',':
                    row.Add(field.ToString());
                    field.Clear();
                    break;
                case '\r':
                    break;
                case '\n':
                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row);
                    row = [];
                    break;
                default:
                    field.Append(c);
                    break;
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        return rows;
    }
}
