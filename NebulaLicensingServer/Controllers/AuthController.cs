using Microsoft.AspNetCore.Mvc;
using NebulaLicensingServer.Common;
using NebulaLicensingServer.Constants;
using NebulaLicensingServer.DTOs.Requests;
using NebulaLicensingServer.DTOs.Responses;
using NebulaLicensingServer.Interfaces;

namespace NebulaLicensingServer.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    /// <summary>Authenticates an administrator and returns JWT tokens.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<LoginResponse>.Fail("Validation failed.", ErrorCodes.ValidationFailed));
        }

        var result = await _authService.AuthenticateAsync(request, cancellationToken);
        if (result.IsFailure || result.Value is null)
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail(result.Error ?? "Unauthorized.", result.ErrorCode));
        }

        return Ok(ApiResponse<LoginResponse>.Ok(result.Value));
    }

    /// <summary>Refreshes an access token using a valid refresh token.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<RefreshTokenResponse>.Fail("Validation failed.", ErrorCodes.ValidationFailed));
        }

        var result = await _authService.RefreshTokenAsync(request, cancellationToken);
        if (result.IsFailure || result.Value is null)
        {
            return Unauthorized(ApiResponse<RefreshTokenResponse>.Fail(result.Error ?? "Unauthorized.", result.ErrorCode));
        }

        return Ok(ApiResponse<RefreshTokenResponse>.Ok(result.Value));
    }
}
