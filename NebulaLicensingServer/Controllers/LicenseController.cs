using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NebulaLicensingServer.Common;
using NebulaLicensingServer.Constants;
using NebulaLicensingServer.DTOs.Licenses.Requests;
using NebulaLicensingServer.DTOs.Licenses.Responses;
using NebulaLicensingServer.Interfaces;

namespace NebulaLicensingServer.Controllers;

[ApiController]
[Route("api/licenses")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.Administrator)]
[Produces("application/json")]
public sealed class LicenseController(ILicenseService licenseService) : ControllerBase
{
    private readonly ILicenseService _licenseService = licenseService;

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LicenseResponse>>> Generate([FromBody] GenerateLicenseRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<LicenseResponse>.Fail("Validation failed.", ErrorCodes.ValidationFailed));
        }

        var result = await _licenseService.GenerateLicenseAsync(request, cancellationToken);
        return HandleLicenseResult(result);
    }

    [HttpPost("activate")]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LicenseResponse>>> Activate([FromBody] ActivateLicenseRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<LicenseResponse>.Fail("Validation failed.", ErrorCodes.ValidationFailed));
        }

        var result = await _licenseService.ActivateLicenseAsync(request, cancellationToken);
        return HandleLicenseResult(result);
    }

    [HttpPost("validate")]
    [ProducesResponseType(typeof(ApiResponse<LicenseValidationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LicenseValidationResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LicenseValidationResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<LicenseValidationResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<LicenseValidationResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LicenseValidationResponse>>> Validate([FromBody] ValidateLicenseRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<LicenseValidationResponse>.Fail("Validation failed.", ErrorCodes.ValidationFailed));
        }

        var result = await _licenseService.ValidateLicenseAsync(request, cancellationToken);
        if (result.IsFailure || result.Value is null)
        {
            return result.Error == "License not found."
                ? NotFound(ApiResponse<LicenseValidationResponse>.Fail(result.Error ?? "License not found.", result.ErrorCode))
                : Conflict(ApiResponse<LicenseValidationResponse>.Fail(result.Error ?? "Validation failed.", result.ErrorCode));
        }

        return Ok(ApiResponse<LicenseValidationResponse>.Ok(result.Value));
    }

    [HttpPost("extend")]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LicenseResponse>>> Extend([FromBody] ExtendLicenseRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<LicenseResponse>.Fail("Validation failed.", ErrorCodes.ValidationFailed));
        }

        var result = await _licenseService.ExtendLicenseAsync(request, cancellationToken);
        return HandleLicenseResult(result);
    }

    [HttpPost("revoke")]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LicenseResponse>>> Revoke([FromBody] RevokeLicenseRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<LicenseResponse>.Fail("Validation failed.", ErrorCodes.ValidationFailed));
        }

        var result = await _licenseService.RevokeLicenseAsync(request, cancellationToken);
        return HandleLicenseResult(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<LicenseResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LicenseResponse>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<LicenseResponse>>>> GetAll([FromQuery] LicenseSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _licenseService.SearchLicensesAsync(request, cancellationToken);
        if (result.IsFailure || result.Value is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<PagedResult<LicenseResponse>>.Fail(result.Error ?? "Unexpected server error.", result.ErrorCode));
        }

        return Ok(ApiResponse<PagedResult<LicenseResponse>>.Ok(result.Value));
    }

    [HttpGet("{licenseKey}")]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<LicenseResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LicenseResponse>>> GetByKey([FromRoute] string licenseKey, CancellationToken cancellationToken)
    {
        var result = await _licenseService.GetLicenseByKeyAsync(licenseKey, cancellationToken);
        if (result.IsFailure || result.Value is null)
        {
            return result.ErrorCode == ErrorCodes.LicenseNotFound
                ? NotFound(ApiResponse<LicenseResponse>.Fail(result.Error ?? "License not found.", result.ErrorCode))
                : StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<LicenseResponse>.Fail(result.Error ?? "Unexpected server error.", result.ErrorCode));
        }

        return Ok(ApiResponse<LicenseResponse>.Ok(result.Value));
    }

    private ActionResult<ApiResponse<LicenseResponse>> HandleLicenseResult(Result<LicenseResponse> result)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return Ok(ApiResponse<LicenseResponse>.Ok(result.Value));
        }

        return result.Error switch
        {
            "License not found." => NotFound(ApiResponse<LicenseResponse>.Fail(result.Error ?? "License not found.", result.ErrorCode)),
            "License is revoked." or "License is expired." or "Cannot extend revoked license." or "Expiration date must move forward." or "License already belongs to another machine." => Conflict(ApiResponse<LicenseResponse>.Fail(result.Error ?? "Conflict.", result.ErrorCode)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<LicenseResponse>.Fail(result.Error ?? "Unexpected server error.", result.ErrorCode))
        };
    }
}
