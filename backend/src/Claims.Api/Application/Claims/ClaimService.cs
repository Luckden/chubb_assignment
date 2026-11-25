using Claims.Api.Domain;
using Claims.Api.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Application.Claims;

/// <summary>
/// One method per use case (docs/design/02-use-cases.md). Loads the claim, calls the domain method that enforces the
/// lifecycle, and saves. The claim's concurrency token turns a lost race into a 409.
/// </summary>
public sealed class ClaimService(
    ClaimsDbContext db,
    TimeProvider clock,
    IValidator<SubmitClaimRequest> submitValidator,
    IValidator<PickUpClaimRequest> pickUpValidator,
    IValidator<RequestInformationRequest> requestInformationValidator,
    IValidator<ProvideInformationRequest> provideInformationValidator,
    IValidator<SettleClaimRequest> settleValidator,
    IValidator<RejectClaimRequest> rejectValidator)
{
    /// <summary>UC-01: a claimant reports an incident.</summary>
    public async Task<ClaimResponse> SubmitAsync(SubmitClaimRequest request, CancellationToken ct)
    {
        await submitValidator.ValidateAndThrowAsync(request, ct);
        await RequireUserAsync(request.ClaimantId, UserRole.Claimant, ct);

        var claim = Claim.Submit(
            request.ClaimantId,
            request.Type,
            request.IncidentDate,
            request.Description.Trim(),
            request.ClaimedAmount,
            clock.GetUtcNow());

        db.Claims.Add(claim);
        await db.SaveChangesAsync(ct);
        return claim.ToResponse();
    }

    /// <summary>UC-02: track a claim, including its history and information requests.</summary>
    public async Task<ClaimResponse> GetAsync(Guid id, CancellationToken ct) =>
        (await LoadAsync(id, ct)).ToResponse();

    /// <summary>
    /// UC-02 and UC-05: list claims, oldest first by submission or by time in current status (RFC-0003). A claimant
    /// filters by their own id; an officer filters by assignee, status or unassigned claims.
    /// </summary>
    public async Task<IReadOnlyList<ClaimSummaryResponse>> ListAsync(
        Guid? claimantId,
        Guid? assigneeId,
        ClaimStatus? status,
        bool? unassigned,
        ClaimListSort sort,
        CancellationToken ct)
    {
        var query = db.Claims.AsNoTracking().AsQueryable();

        if (claimantId is { } claimant)
        {
            query = query.Where(c => c.ClaimantId == claimant);
        }

        if (assigneeId is { } assignee)
        {
            query = query.Where(c => c.AssigneeId == assignee);
        }

        if (status is { } s)
        {
            query = query.Where(c => c.Status == s);
        }

        if (unassigned == true)
        {
            query = query.Where(c => c.AssigneeId == null);
        }

        // The status history is the only record of when the status changed (ADR-0014), so no column is added.
        query = sort == ClaimListSort.StatusChangedAt
            ? query.OrderBy(c => c.StatusHistory.Max(h => h.ChangedAt)).ThenBy(c => c.SubmittedAt)
            : query.OrderBy(c => c.SubmittedAt);

        return await query
            .Select(c => new ClaimSummaryResponse(
                c.Id,
                c.ClaimNumber,
                c.ClaimantId,
                c.AssigneeId,
                c.Type,
                c.Status,
                c.ClaimedAmount,
                c.SubmittedAt,
                c.StatusHistory.Max(h => h.ChangedAt)))
            .ToListAsync(ct);
    }

    /// <summary>UC-05: an officer picks up an incoming claim.</summary>
    public async Task<ClaimResponse> PickUpAsync(Guid id, PickUpClaimRequest request, CancellationToken ct)
    {
        await pickUpValidator.ValidateAndThrowAsync(request, ct);
        await RequireUserAsync(request.OfficerId, UserRole.Officer, ct);

        var claim = await LoadAsync(id, ct);
        claim.PickUp(request.OfficerId, clock.GetUtcNow());
        await db.SaveChangesAsync(ct);
        return claim.ToResponse();
    }

    /// <summary>UC-06: the assigned officer asks the claimant for more information.</summary>
    public async Task<ClaimResponse> RequestInformationAsync(Guid id, RequestInformationRequest request, CancellationToken ct)
    {
        await requestInformationValidator.ValidateAndThrowAsync(request, ct);
        await RequireUserAsync(request.OfficerId, UserRole.Officer, ct);

        var claim = await LoadAsync(id, ct);
        claim.RequestInformation(request.OfficerId, request.Question.Trim(), clock.GetUtcNow());
        await db.SaveChangesAsync(ct);
        return claim.ToResponse();
    }

    /// <summary>UC-03: the claimant provides the requested information.</summary>
    public async Task<ClaimResponse> ProvideInformationAsync(Guid id, ProvideInformationRequest request, CancellationToken ct)
    {
        await provideInformationValidator.ValidateAndThrowAsync(request, ct);
        await RequireUserAsync(request.ClaimantId, UserRole.Claimant, ct);

        var claim = await LoadAsync(id, ct);
        claim.ProvideInformation(request.ClaimantId, request.Response.Trim(), clock.GetUtcNow());
        await db.SaveChangesAsync(ct);
        return claim.ToResponse();
    }

    /// <summary>UC-07: the assigned officer settles the claim.</summary>
    public async Task<ClaimResponse> SettleAsync(Guid id, SettleClaimRequest request, CancellationToken ct)
    {
        await settleValidator.ValidateAndThrowAsync(request, ct);
        await RequireUserAsync(request.OfficerId, UserRole.Officer, ct);

        var claim = await LoadAsync(id, ct);
        claim.Settle(request.OfficerId, request.Reason?.Trim(), clock.GetUtcNow());
        await db.SaveChangesAsync(ct);
        return claim.ToResponse();
    }

    /// <summary>UC-07: the assigned officer rejects the claim.</summary>
    public async Task<ClaimResponse> RejectAsync(Guid id, RejectClaimRequest request, CancellationToken ct)
    {
        await rejectValidator.ValidateAndThrowAsync(request, ct);
        await RequireUserAsync(request.OfficerId, UserRole.Officer, ct);

        var claim = await LoadAsync(id, ct);
        claim.Reject(request.OfficerId, request.Reason.Trim(), clock.GetUtcNow());
        await db.SaveChangesAsync(ct);
        return claim.ToResponse();
    }

    private async Task<Claim> LoadAsync(Guid id, CancellationToken ct) =>
        await db.Claims
            .Include(c => c.StatusHistory)
            .Include(c => c.InformationRequests)
            .SingleOrDefaultAsync(c => c.Id == id, ct)
        ?? throw new NotFoundException($"Claim {id} was not found.");

    private async Task RequireUserAsync(Guid userId, UserRole role, CancellationToken ct)
    {
        if (!await db.Users.AnyAsync(u => u.Id == userId && u.Role == role, ct))
        {
            throw new NotPermittedException($"User {userId} is not a known {role}.");
        }
    }
}
