using Claims.Api.Domain;

namespace Claims.Api.Application.Claims;

/// <summary>
/// A row in a claim list: the claimant's own claims, or the officer's queue. <see cref="StatusChangedAt"/> is when
/// the claim entered its current status (latest status history row), so the queue can show what is going stale.
/// </summary>
public sealed record ClaimSummaryResponse(
    Guid Id,
    string ClaimNumber,
    Guid ClaimantId,
    Guid? AssigneeId,
    ClaimType Type,
    ClaimStatus Status,
    decimal ClaimedAmount,
    DateTimeOffset SubmittedAt,
    DateTimeOffset StatusChangedAt);

/// <summary>Order of a claim list. Both are oldest first.</summary>
public enum ClaimListSort
{
    SubmittedAt,
    StatusChangedAt,
}

/// <summary>R2: a claim with its history and information requests, so the claimant can track it.</summary>
public sealed record ClaimResponse(
    Guid Id,
    string ClaimNumber,
    Guid ClaimantId,
    Guid? AssigneeId,
    ClaimType Type,
    ClaimStatus Status,
    DateOnly IncidentDate,
    string Description,
    decimal ClaimedAmount,
    DecisionOutcome? DecisionOutcome,
    string? DecisionReason,
    DateTimeOffset? DecidedAt,
    DateTimeOffset SubmittedAt,
    IReadOnlyList<ClaimStatusChangeResponse> StatusHistory,
    IReadOnlyList<InformationRequestResponse> InformationRequests);

public sealed record ClaimStatusChangeResponse(
    ClaimStatus? FromStatus,
    ClaimStatus ToStatus,
    Guid ChangedBy,
    DateTimeOffset ChangedAt,
    string? Note);

public sealed record InformationRequestResponse(
    Guid Id,
    Guid RequestedBy,
    string Question,
    string? Response,
    DateTimeOffset RequestedAt,
    DateTimeOffset? RespondedAt);
