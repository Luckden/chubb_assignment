using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

/// <summary>Basic service information. Placeholder endpoint proving the API → OpenAPI → frontend chain.</summary>
public sealed record SystemInfoResponse(string Service, string Environment, string? Version);

[ApiController]
[Route("api/system")]
public class SystemController(IHostEnvironment environment) : ControllerBase
{
    [HttpGet("info")]
    [ProducesResponseType<SystemInfoResponse>(StatusCodes.Status200OK)]
    public ActionResult<SystemInfoResponse> GetInfo() => new SystemInfoResponse(
        Service: "claims-api",
        Environment: environment.EnvironmentName,
        Version: typeof(Program).Assembly.GetName().Version?.ToString());
}
