using Claims.Api.Domain;
using Claims.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Application.Workload;

/// <summary>
/// R8 / D6: team workload and performance, computed on each read from claims and their status history (ADR-0014).
/// Rows are few at this scale, so the database filters and the figures are grouped in memory.
/// </summary>
public sealed class WorkloadService(ClaimsDbContext db, TimeProvider clock)
{
    /// <summary>Performance looks back this many days (RFC-0003 section 2).</summary>
    public const int PeriodDays = 30;

    public async Task<WorkloadResponse> GetAsync(Guid officerId, CancellationToken ct)
    {
        var acting = await ActingOfficer.RequireAsync(db, officerId, ct);
        var now = clock.GetUtcNow();
        var periodStart = now.AddDays(-PeriodDays);

        var team = await db.Users.AsNoTracking()
            .Where(u => u.Role == UserRole.Officer && u.TeamId == acting.TeamId)
            .OrderBy(u => u.Name)
            .Select(u => new { u.Id, u.Name })
            .ToListAsync(ct);
        var teamIds = team.Select(o => o.Id).ToList();

        var open = await db.Claims.AsNoTracking()
            .Where(c => c.AssigneeId != null && teamIds.Contains(c.AssigneeId.Value)
                && (c.Status == ClaimStatus.UnderReview || c.Status == ClaimStatus.InfoRequested))
            .Select(c => new { OfficerId = c.AssigneeId!.Value, c.Status, StatusChangedAt = c.StatusHistory.Max(h => h.ChangedAt) })
            .ToListAsync(ct);

        var decided = await db.Claims.AsNoTracking()
            .Where(c => c.AssigneeId != null && teamIds.Contains(c.AssigneeId.Value)
                && c.DecidedAt != null && c.DecidedAt >= periodStart)
            .Select(c => new { OfficerId = c.AssigneeId!.Value, c.DecisionOutcome, c.SubmittedAt, DecidedAt = c.DecidedAt!.Value })
            .ToListAsync(ct);

        var waiting = db.Claims.AsNoTracking().Where(c => c.Status == ClaimStatus.Submitted && c.AssigneeId == null);
        var unassigned = new UnassignedWorkloadResponse(
            await waiting.CountAsync(ct),
            await waiting.MinAsync(c => (DateTimeOffset?)c.SubmittedAt, ct));

        var officers = team.Select(o =>
        {
            var mine = open.Where(c => c.OfficerId == o.Id).ToList();
            var myDecisions = decided.Where(c => c.OfficerId == o.Id).ToList();
            return new OfficerWorkloadResponse(
                o.Id,
                o.Name,
                mine.Count(c => c.Status == ClaimStatus.UnderReview),
                mine.Count(c => c.Status == ClaimStatus.InfoRequested),
                mine.Count == 0 ? null : mine.Min(c => c.StatusChangedAt),
                myDecisions.Count,
                myDecisions.Count(c => c.DecisionOutcome == DecisionOutcome.Settled),
                myDecisions.Count(c => c.DecisionOutcome == DecisionOutcome.Rejected),
                myDecisions.Count == 0
                    ? null
                    : Math.Round(myDecisions.Average(c => (c.DecidedAt - c.SubmittedAt).TotalDays), 1));
        }).ToList();

        return new WorkloadResponse(acting.TeamId, now, PeriodDays, unassigned, officers);
    }
}
