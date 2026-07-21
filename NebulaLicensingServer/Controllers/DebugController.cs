using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NebulaLicensingServer.Common;

namespace NebulaLicensingServer.Controllers;

[ApiController]
[Route("api/debug")]
[Produces("application/json")]
public sealed class DebugController : ControllerBase
{
    [HttpGet("claims")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<object>>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<IReadOnlyList<object>>> GetClaims()
    {
        var claims = User.Claims
            .Select(claim => new
            {
                Type = claim.Type,
                Value = claim.Value
            })
            .ToList();

        return Ok(ApiResponse<IReadOnlyList<object>>.Ok(claims));
    }
}
