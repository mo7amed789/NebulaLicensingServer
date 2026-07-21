using NebulaLicensingServer.Common;
using NebulaLicensingServer.DTOs.AuditLogs.Requests;
using NebulaLicensingServer.DTOs.AuditLogs.Responses;
using NebulaLicensingServer.DTOs.Licenses.Responses;

namespace NebulaLicensingServer.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(string action, string? administrator, string? licenseKey, string result, string? ipAddress = null, string? details = null, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<AuditLogResponse>>> SearchAsync(AuditLogSearchRequest request, CancellationToken cancellationToken = default);
}
