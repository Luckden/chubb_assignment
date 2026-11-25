using Claims.Api.Domain;

namespace Claims.Api.Application.Exposure;

/// <summary>
/// D7: claimed exposure, the sum of <c>claimed_amount</c> on claims not yet decided (ADR-0015). An upper bound on
/// what will be paid, not a reserve. Every status, type and age bucket is listed, with zeros where there is nothing.
/// </summary>
public sealed record ExposureResponse(
    DateTimeOffset AsOf,
    int OpenClaims,
    decimal ClaimedExposure,
    IReadOnlyList<ExposureByStatusResponse> ByStatus,
    IReadOnlyList<ExposureByTypeResponse> ByType,
    IReadOnlyList<ExposureByAgeResponse> ByAge);

public sealed record ExposureByStatusResponse(ClaimStatus Status, int Count, decimal ClaimedAmount);

public sealed record ExposureByTypeResponse(ClaimType Type, int Count, decimal ClaimedAmount);

/// <summary>Days since submission. Display buckets, not deadlines (ADR-0012).</summary>
public sealed record ExposureByAgeResponse(string Bucket, int Count, decimal ClaimedAmount);
