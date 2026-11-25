namespace Claims.Api.Infrastructure.Messaging;

/// <summary>Central list of Kafka topic names (RFC-0002).</summary>
public static class Topics
{
    /// <summary>Every claim lifecycle event, keyed by claim id so one claim's events stay in order.</summary>
    public const string ClaimEvents = "claims.claim-events";

    /// <summary>Messages the consumer could not handle after its retries, with the error in a header.</summary>
    public const string ClaimEventsDeadLetter = "claims.claim-events.dlt";

    public static readonly string[] All = [ClaimEvents, ClaimEventsDeadLetter];
}
