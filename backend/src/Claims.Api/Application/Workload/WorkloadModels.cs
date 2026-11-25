namespace Claims.Api.Application.Workload;

/// <summary>R8: the acting officer's team, its open work and its decisions in the last <see cref="PeriodDays"/> days.</summary>
public sealed record WorkloadResponse(
    string? TeamId,
    DateTimeOffset AsOf,
    int PeriodDays,
    UnassignedWorkloadResponse Unassigned,
    IReadOnlyList<OfficerWorkloadResponse> Officers);

/// <summary>Claims nobody has picked up yet. A team-level figure: they belong to no officer.</summary>
public sealed record UnassignedWorkloadResponse(int Count, DateTimeOffset? OldestSubmittedAt);

/// <summary>
/// One officer's open work (workload) and decisions in the period (performance). The average runs from submission to
/// decision over the decisions in the period, and is null when there are none.
/// </summary>
public sealed record OfficerWorkloadResponse(
    Guid OfficerId,
    string Name,
    int UnderReview,
    int InfoRequested,
    DateTimeOffset? OldestOpenStatusChangedAt,
    int DecidedInPeriod,
    int SettledInPeriod,
    int RejectedInPeriod,
    double? AverageDaysToDecision);
