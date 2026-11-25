using Claims.Api.Domain;

namespace Claims.Api.Application.Claims;

internal static class ClaimMapping
{
    public static ClaimResponse ToResponse(this Claim claim) => new(
        claim.Id,
        claim.ClaimNumber,
        claim.ClaimantId,
        claim.AssigneeId,
        claim.Type,
        claim.Status,
        claim.IncidentDate,
        claim.Description,
        claim.ClaimedAmount,
        claim.DecisionOutcome,
        claim.DecisionReason,
        claim.DecidedAt,
        claim.SubmittedAt,
        [.. claim.StatusHistory
            .OrderBy(h => h.ChangedAt)
            .Select(h => new ClaimStatusChangeResponse(h.FromStatus, h.ToStatus, h.ChangedBy, h.ChangedAt, h.Note))],
        [.. claim.InformationRequests
            .OrderBy(r => r.RequestedAt)
            .Select(r => new InformationRequestResponse(r.Id, r.RequestedBy, r.Question, r.Response, r.RequestedAt, r.RespondedAt))]);
}
