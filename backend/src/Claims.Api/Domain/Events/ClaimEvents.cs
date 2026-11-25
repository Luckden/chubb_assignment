using System.Text.Json.Serialization;

namespace Claims.Api.Domain.Events;

/// <summary>
/// A past-tense fact about a claim, raised by <see cref="Claim"/> when it changes state (RFC-0002). Plain records, no
/// framework types. The envelope (event id, type, version, claim id, time) is added when the event is written to the
/// outbox, so <see cref="ClaimId"/> and <see cref="OccurredAt"/> are not part of the serialised <c>data</c>.
/// </summary>
public abstract record ClaimDomainEvent
{
    [JsonIgnore]
    public Guid ClaimId { get; init; }

    [JsonIgnore]
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>The event name on the wire, for example <c>ClaimSubmitted</c>.</summary>
    [JsonIgnore]
    public string EventType => GetType().Name;
}

public sealed record ClaimSubmitted(
    string ClaimNumber,
    Guid ClaimantId,
    ClaimType Type,
    DateOnly IncidentDate,
    decimal ClaimedAmount) : ClaimDomainEvent;

public sealed record ClaimPickedUp(Guid AssigneeId) : ClaimDomainEvent;

public sealed record InformationRequested(Guid InformationRequestId, string Question) : ClaimDomainEvent;

public sealed record InformationProvided(Guid InformationRequestId) : ClaimDomainEvent;

public sealed record ClaimSettled(string? Reason) : ClaimDomainEvent;

public sealed record ClaimRejected(string Reason) : ClaimDomainEvent;
