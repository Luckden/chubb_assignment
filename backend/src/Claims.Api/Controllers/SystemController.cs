using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

/// <summary>Placeholder endpoint proving the API is wired up. Replace with domain controllers.</summary>
[ApiController]
[Route("api/system")]
public class SystemController(IHostEnvironment environment) : ControllerBase
{
    [HttpGet("info")]
    public IActionResult GetInfo() => Ok(new
    {
        service = "claims-api",
        environment = environment.EnvironmentName,
        version = typeof(Program).Assembly.GetName().Version?.ToString(),
    });
}
