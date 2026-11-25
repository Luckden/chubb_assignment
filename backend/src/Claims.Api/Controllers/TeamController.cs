using Claims.Api.Application.Exposure;
using Claims.Api.Application.Workload;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

/// <summary>Read-only team views for officers (RFC-0003): workload and performance (R8), claimed exposure (D7).</summary>
[ApiController]
[Route("api")]
public class TeamController(WorkloadService workload, ExposureService exposure) : ControllerBase
{
    /// <summary>The acting officer's team: open work per officer and decisions in the last 30 days (R8).</summary>
    [HttpGet("workload")]
    [ProducesResponseType<WorkloadResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WorkloadResponse>> Workload([FromQuery] Guid officerId, CancellationToken ct) =>
        await workload.GetAsync(officerId, ct);

    /// <summary>Claimed exposure: the claimed amount on claims not yet decided, an upper bound on liability (D7).</summary>
    [HttpGet("exposure")]
    [ProducesResponseType<ExposureResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ExposureResponse>> Exposure([FromQuery] Guid officerId, CancellationToken ct) =>
        await exposure.GetAsync(officerId, ct);
}
