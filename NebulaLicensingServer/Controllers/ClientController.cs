using Microsoft.AspNetCore.Mvc;
using NebulaLicensingServer.Common;
using NebulaLicensingServer.Constants;
using NebulaLicensingServer.DTOs.Client.Requests;
using NebulaLicensingServer.DTOs.Client.Responses;
using NebulaLicensingServer.Interfaces;
using NebulaLicensingServer.Services;

namespace NebulaLicensingServer.Controllers;

[ApiController]
[Route("api/client")]
[Produces("application/json")]
// تم إضافة LicenseSigningService للحاضنة (Constructor) هنا ليكون الكود أكثر التزاماً بمبادئ SOLID
public sealed class ClientController(
    ILicenseService licenseService,
    IAuditLogService auditLogService,
    LicenseSigningService licenseSigningService) : ControllerBase
{
    [HttpPost("activate")]
    [ProducesResponseType(typeof(ApiResponse<ClientActivationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClientActivationResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ClientActivationResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ClientActivationResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ClientActivationResponse>>> Activate([FromBody] ClientActivateRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<ClientActivationResponse>.Fail("Validation failed.", ErrorCodes.ValidationFailed));
        }

        var result = await licenseService.ActivateLicenseAsync(new DTOs.Licenses.Requests.ActivateLicenseRequest
        {
            LicenseKey = request.LicenseKey,
            MachineHash = request.MachineHash,
            DeviceName = request.DeviceName
        }, cancellationToken);

        if (result.IsFailure || result.Value is null)
        {
            await auditLogService.LogAsync("Activation", null, request.LicenseKey, "Rejected", cancellationToken: cancellationToken);
            return await ToActivationFailureResultAsync(request.LicenseKey, result, cancellationToken);
        }

        await auditLogService.LogAsync("Activation", null, request.LicenseKey, "Success", cancellationToken: cancellationToken);

        // استخدام الخدمة المحقونة بدلاً من HttpContext
        var signature = licenseSigningService.Sign(
            result.Value.LicenseKey,
            result.Value.MachineHash ?? string.Empty,
            result.Value.DeviceName ?? string.Empty,
            request.NebulaVersion,
            request.ServerId,
            result.Value.ExpiresAt.GetValueOrDefault(),
            true);

        return Ok(ApiResponse<ClientActivationResponse>.Ok(new ClientActivationResponse
        {
            Success = true,
            Status = result.Value.Status.ToString(),
            ExpiresAt = result.Value.ExpiresAt.GetValueOrDefault(),
            LicenseKey = result.Value.LicenseKey,
            MachineName = result.Value.DeviceName ?? string.Empty,

            Signature = signature.Signature,
            SignatureAlgorithm = signature.SignatureAlgorithm,
            SignedAtUtc = signature.SignedAtUtc,
            OfflineGraceUntilUtc = signature.OfflineGraceUntilUtc
        }));
    }

    [HttpPost("validate")]
    [ProducesResponseType(typeof(ApiResponse<ClientValidationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClientValidationResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ClientValidationResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ClientValidationResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ClientValidationResponse>>> Validate([FromBody] ClientValidateRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<ClientValidationResponse>.Fail("Validation failed.", ErrorCodes.ValidationFailed));
        }

        var result = await licenseService.ValidateLicenseAsync(new DTOs.Licenses.Requests.ValidateLicenseRequest
        {
            LicenseKey = request.LicenseKey,
            MachineHash = request.MachineHash
        }, cancellationToken);

        if (result.Value is null || result.Value.IsValid is false)
        {
            await auditLogService.LogAsync("Validation", null, request.LicenseKey, "Rejected", cancellationToken: cancellationToken);
            return await ToValidationFailureResultAsync(request.LicenseKey, result, cancellationToken);
        }

        var source = await licenseService.GetLicenseByKeyAsync(request.LicenseKey, cancellationToken);

        // التحقق أولاً لتفادي NullReferenceException
        if (source.IsFailure || source.Value is null)
        {
            return source.ErrorCode == ErrorCodes.LicenseNotFound
                ? NotFound(ApiResponse<ClientValidationResponse>.Fail(source.Error ?? "License not found.", source.ErrorCode))
                : StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<ClientValidationResponse>.Fail(source.Error ?? "Unexpected server error.", source.ErrorCode));
        }

        // تم تنظيف الكود المكرر ونقل التوقيع هنا بعد التأكد من أن source.Value ليست Null
        var signature = licenseSigningService.Sign(
            source.Value.LicenseKey,
            source.Value.MachineHash ?? string.Empty,
            source.Value.DeviceName ?? string.Empty,
            request.NebulaVersion,
            request.ServerId,
            source.Value.ExpiresAt.GetValueOrDefault(),
            true);

        await auditLogService.LogAsync("Validation", null, request.LicenseKey, "Success", cancellationToken: cancellationToken);

        return Ok(ApiResponse<ClientValidationResponse>.Ok(new ClientValidationResponse
        {
            IsValid = true,
            Status = source.Value.Status.ToString(),
            ExpiresAt = source.Value.ExpiresAt.GetValueOrDefault(),
            DaysRemaining = source.Value.ExpiresAt.HasValue
                ? Math.Max(0, (source.Value.ExpiresAt.Value - DateTime.UtcNow).Days)
                : 0,
            Message = "License is valid.",

            Signature = signature.Signature,
            SignatureAlgorithm = signature.SignatureAlgorithm,
            SignedAtUtc = signature.SignedAtUtc,
            OfflineGraceUntilUtc = signature.OfflineGraceUntilUtc
        }));
    } // تم إغلاق قوس دالة Validate هنا بشكل صحيح!

    [HttpPost("heartbeat")]
    [ProducesResponseType(typeof(ApiResponse<ClientHeartbeatResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClientHeartbeatResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ClientHeartbeatResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ClientHeartbeatResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ClientHeartbeatResponse>>> Heartbeat([FromBody] ClientHeartbeatRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<ClientHeartbeatResponse>.Fail("Validation failed.", ErrorCodes.ValidationFailed));
        }

        var result = await licenseService.HeartbeatAsync(request.LicenseKey, request.MachineHash, cancellationToken);
        if (result.IsFailure || result.Value is null)
        {
            await auditLogService.LogAsync("Heartbeat", null, request.LicenseKey, "Rejected", cancellationToken: cancellationToken);
            return await ToHeartbeatFailureResultAsync(request.LicenseKey, result, cancellationToken);
        }

        await auditLogService.LogAsync("Heartbeat", null, request.LicenseKey, "Success", cancellationToken: cancellationToken);
        return Ok(ApiResponse<ClientHeartbeatResponse>.Ok(new ClientHeartbeatResponse
        {
            CurrentServerTimeUtc = DateTime.UtcNow,
            Status = result.Value.Status.ToString()
        }));
    }

    private async Task<ActionResult<ApiResponse<ClientActivationResponse>>> ToActivationFailureResultAsync(
        string licenseKey,
        Result<DTOs.Licenses.Responses.LicenseResponse> result,
        CancellationToken cancellationToken)
    {
        var message = result.Error ?? "Conflict.";
        var source = await licenseService.GetLicenseByKeyAsync(licenseKey, cancellationToken);

        var data = new ClientActivationResponse
        {
            Success = false,
            Status = MapClientStatus(result.Error),
            ExpiresAt = source.Value?.ExpiresAt ?? default,
            LicenseKey = source.Value?.LicenseKey ?? licenseKey,
            MachineName = null
        };

        return CreateFailureResult(result.ErrorCode == ErrorCodes.LicenseNotFound ? StatusCodes.Status404NotFound : StatusCodes.Status409Conflict, message, result.ErrorCode, data);
    }

    private async Task<ActionResult<ApiResponse<ClientValidationResponse>>> ToValidationFailureResultAsync(
        string licenseKey,
        Result<DTOs.Licenses.Responses.LicenseValidationResponse> result,
        CancellationToken cancellationToken)
    {
        var message = result.Value?.Message ?? result.Error ?? "Validation failed.";
        var source = await licenseService.GetLicenseByKeyAsync(licenseKey, cancellationToken);

        var data = new ClientValidationResponse
        {
            IsValid = false,
            Status = MapClientStatus(message),
            ExpiresAt = source.Value?.ExpiresAt ?? default,
            DaysRemaining = source.Value?.ExpiresAt is null ? 0 : Math.Max(0, (source.Value.ExpiresAt.Value - DateTime.UtcNow).Days),
            Message = message
        };

        return CreateFailureResult(result.ErrorCode == ErrorCodes.LicenseNotFound ? StatusCodes.Status404NotFound : StatusCodes.Status409Conflict, message, result.ErrorCode, data);
    }

    private async Task<ActionResult<ApiResponse<ClientHeartbeatResponse>>> ToHeartbeatFailureResultAsync(
        string licenseKey,
        Result<DTOs.Licenses.Responses.LicenseResponse> result,
        CancellationToken cancellationToken)
    {
        var message = result.Error ?? "Rejected.";
        var data = new ClientHeartbeatResponse
        {
            CurrentServerTimeUtc = DateTime.UtcNow,
            Status = MapHeartbeatStatus(message)
        };

        return CreateFailureResult(result.ErrorCode == ErrorCodes.LicenseNotFound ? StatusCodes.Status404NotFound : StatusCodes.Status409Conflict, message, result.ErrorCode, data);
    }

    private static ObjectResult CreateFailureResult<T>(int statusCode, string message, string? errorCode, T data) =>
        new(new
        {
            success = false,
            message,
            errorCode,
            timestamp = DateTime.UtcNow,
            data
        })
        {
            StatusCode = statusCode
        };

    private static string MapClientStatus(string? message) =>
        message?.Contains("revoked", StringComparison.OrdinalIgnoreCase) == true ? "Revoked" :
        message?.Contains("expired", StringComparison.OrdinalIgnoreCase) == true ? "Expired" :
        message?.Contains("machine", StringComparison.OrdinalIgnoreCase) == true ? "MachineMismatch" :
        "ValidationFailed";

    private static string MapHeartbeatStatus(string? message) =>
        message?.Contains("revoked", StringComparison.OrdinalIgnoreCase) == true ? "Revoked" :
        message?.Contains("expired", StringComparison.OrdinalIgnoreCase) == true ? "Expired" :
        message?.Contains("machine", StringComparison.OrdinalIgnoreCase) == true ? "MachineMismatch" :
        "ValidationFailed";
}