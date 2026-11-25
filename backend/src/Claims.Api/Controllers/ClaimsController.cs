using Claims.Api.Application.Claims;
using Claims.Api.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

/// <summary>Thin: binds the request, calls <see cref="ClaimService"/>, and returns the result. Errors become ProblemDetails.</summary>
[ApiController]
[Route("api/claims")]
public class ClaimsController(ClaimService claims) : ControllerBase
{
    /// <summary>Report an incident (R1).</summary>
    [HttpPost]
    [ProducesResponseType<ClaimResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ClaimResponse>> Submit(SubmitClaimRequest request, CancellationToken ct)
    {
        var claim = await claims.SubmitAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = claim.Id }, claim);
    }

    /// <summary>List claims, oldest first. Filter by claimant, assignee, status, or unassigned claims.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClaimSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClaimSummaryResponse>>> List(
        [FromQuery] Guid? claimantId,
        [FromQuery] Guid? assigneeId,
        [FromQuery] ClaimStatus? status,
        [FromQuery] bool? unassigned,
        CancellationToken ct) =>
        Ok(await claims.ListAsync(claimantId, assigneeId, status, unassigned, ct));

    /// <summary>Track a claim (R2).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ClaimResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimResponse>> Get(Guid id, CancellationToken ct) =>
        await claims.GetAsync(id, ct);

    /// <summary>An officer picks up an incoming claim (R5).</summary>
    [HttpPost("{id:guid}/pick-up")]
    [ProducesResponseType<ClaimResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClaimResponse>> PickUp(Guid id, PickUpClaimRequest request, CancellationToken ct) =>
        await claims.PickUpAsync(id, request, ct);

    /// <summary>The assigned officer asks the claimant for more information (R6).</summary>
    [HttpPost("{id:guid}/information-requests")]
    [ProducesResponseType<ClaimResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClaimResponse>> RequestInformation(
        Guid id, RequestInformationRequest request, CancellationToken ct) =>
        await claims.RequestInformationAsync(id, request, ct);

    /// <summary>The claimant answers the open information request (R3).</summary>
    [HttpPost("{id:guid}/information-response")]
    [ProducesResponseType<ClaimResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClaimResponse>> ProvideInformation(
        Guid id, ProvideInformationRequest request, CancellationToken ct) =>
        await claims.ProvideInformationAsync(id, request, ct);

    /// <summary>The assigned officer settles the claim (R7).</summary>
    [HttpPost("{id:guid}/settle")]
    [ProducesResponseType<ClaimResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClaimResponse>> Settle(Guid id, SettleClaimRequest request, CancellationToken ct) =>
        await claims.SettleAsync(id, request, ct);

    /// <summary>The assigned officer rejects the claim, giving a reason (R7).</summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType<ClaimResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClaimResponse>> Reject(Guid id, RejectClaimRequest request, CancellationToken ct) =>
        await claims.RejectAsync(id, request, ct);
}
