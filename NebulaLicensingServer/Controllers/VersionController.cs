using Microsoft.AspNetCore.Mvc;
using NebulaLicensingServer.Common;

namespace NebulaLicensingServer.Controllers;

[ApiController]
[Route("api/version")]
public sealed class VersionController(IHostEnvironment environment) : ControllerBase
{
    private readonly IHostEnvironment _environment = environment;

    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        var response = ApiResponse<object>.Ok(new
        {
            ApplicationName = "Nebula Licensing Server",
            Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Environment = _environment.EnvironmentName,
            BuildDate = System.IO.File.GetLastWriteTimeUtc(typeof(Program).Assembly.Location)
        });

        return Ok(response);
    }
}
