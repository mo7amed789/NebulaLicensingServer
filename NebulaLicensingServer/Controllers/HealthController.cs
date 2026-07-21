using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NebulaLicensingServer.Common;
using NebulaLicensingServer.Persistence;

namespace NebulaLicensingServer.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController(ApplicationDbContext context) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> Get(CancellationToken cancellationToken)
    {
        var databaseOk = await _context.Database.CanConnectAsync(cancellationToken);
        var response = ApiResponse<object>.Ok(new
        {
            Application = true,
            Database = databaseOk
        });

        return databaseOk ? Ok(response) : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}
