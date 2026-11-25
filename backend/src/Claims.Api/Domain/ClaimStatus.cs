namespace Claims.Api.Domain;

/// <summary>Lifecycle of a claim. Allowed transitions are documented in docs/design/04-claim-lifecycle.md.</summary>
public enum ClaimStatus
{
    Submitted,
    UnderReview,
    InfoRequested,
    Settled,
    Rejected,
}
