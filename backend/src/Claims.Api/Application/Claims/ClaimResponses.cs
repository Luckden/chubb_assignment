using Claims.Api.Domain;

namespace Claims.Api.Application.Claims;

/// <summary>A row in a claim list: the claimant's own claims, or the officer's queue.</summary>
public sealed record ClaimSummaryResponse(
    Guid Id,
    string ClaimNumber,
    Guid ClaimantId,
    Guid? AssigneeId,
    ClaimType Type,
    ClaimStatus Status,
    decimal ClaimedAmount,
    DateTimeOffset SubmittedAt);

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
