using Claims.Api.Domain;
using Claims.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Application.Exposure;

/// <summary>D7: claimed exposure over the whole book, computed on each read (ADR-0014, ADR-0015).</summary>
public sealed class ExposureService(ClaimsDbContext db, TimeProvider clock)
{
    public const string UpToAWeek = "0-7 days";
    public const string UpToAMonth = "8-30 days";
    public const string OverAMonth = "31+ days";

    private static readonly ClaimStatus[] OpenStatuses =
        [ClaimStatus.Submitted, ClaimStatus.UnderReview, ClaimStatus.InfoRequested];

    private static readonly string[] AgeBuckets = [UpToAWeek, UpToAMonth, OverAMonth];

    /// <summary>The age bucket for a claim submitted <paramref name="days"/> whole days ago.</summary>
    public static string AgeBucket(int days) => days switch
    {
        <= 7 => UpToAWeek,
        <= 30 => UpToAMonth,
        _ => OverAMonth,
    };

    public async Task<ExposureResponse> GetAsync(Guid officerId, CancellationToken ct)
    {
        await ActingOfficer.RequireAsync(db, officerId, ct);
        var now = clock.GetUtcNow();

        var open = await db.Claims.AsNoTracking()
            .Where(c => OpenStatuses.Contains(c.Status))
            .Select(c => new { c.Status, c.Type, c.ClaimedAmount, c.SubmittedAt })
            .ToListAsync(ct);

        var byStatus = OpenStatuses
            .Select(s => (Status: s, Rows: open.Where(c => c.Status == s).ToList()))
            .Select(x => new ExposureByStatusResponse(x.Status, x.Rows.Count, x.Rows.Sum(c => c.ClaimedAmount)))
            .ToList();

        var byType = Enum.GetValues<ClaimType>()
            .Select(t => (Type: t, Rows: open.Where(c => c.Type == t).ToList()))
            .Select(x => new ExposureByTypeResponse(x.Type, x.Rows.Count, x.Rows.Sum(c => c.ClaimedAmount)))
            .ToList();

        var byAge = AgeBuckets
            .Select(b => (Bucket: b, Rows: open.Where(c => AgeBucket((int)(now - c.SubmittedAt).TotalDays) == b).ToList()))
            .Select(x => new ExposureByAgeResponse(x.Bucket, x.Rows.Count, x.Rows.Sum(c => c.ClaimedAmount)))
            .ToList();

        return new ExposureResponse(now, open.Count, open.Sum(c => c.ClaimedAmount), byStatus, byType, byAge);
    }
}
