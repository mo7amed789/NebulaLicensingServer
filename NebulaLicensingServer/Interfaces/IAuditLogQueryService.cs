using NebulaLicensingServer.Common;
using NebulaLicensingServer.DTOs.AuditLogs.Requests;
using NebulaLicensingServer.DTOs.AuditLogs.Responses;
using NebulaLicensingServer.DTOs.Licenses.Responses;

namespace NebulaLicensingServer.Interfaces;

public interface IAuditLogQueryService
{
    Task<Result<PagedResult<AuditLogResponse>>> SearchAsync(AuditLogSearchRequest request, CancellationToken cancellationToken = default);
}
