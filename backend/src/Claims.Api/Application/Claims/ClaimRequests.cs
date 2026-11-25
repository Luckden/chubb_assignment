using Claims.Api.Domain;

namespace Claims.Api.Application.Claims;

// There is no authentication yet (RFC-0001: auth is not decided), so every request names the acting user.
// The service checks that the user exists and has the right role. Replace with the signed-in user when auth lands.

/// <summary>R1: report an incident.</summary>
public sealed record SubmitClaimRequest(
    Guid ClaimantId,
    ClaimType Type,
    DateOnly IncidentDate,
    string Description,
    decimal ClaimedAmount);

/// <summary>R5: an officer picks up a claim.</summary>
public sealed record PickUpClaimRequest(Guid OfficerId);

/// <summary>R6: an officer asks the claimant for more information.</summary>
public sealed record RequestInformationRequest(Guid OfficerId, string Question);

/// <summary>R3: the claimant answers the open information request.</summary>
public sealed record ProvideInformationRequest(Guid ClaimantId, string Response);

/// <summary>R7: an officer settles a claim. The reason is optional.</summary>
public sealed record SettleClaimRequest(Guid OfficerId, string? Reason);

/// <summary>R7: an officer rejects a claim. The reason is required and is shown to the claimant.</summary>
public sealed record RejectClaimRequest(Guid OfficerId, string Reason);
