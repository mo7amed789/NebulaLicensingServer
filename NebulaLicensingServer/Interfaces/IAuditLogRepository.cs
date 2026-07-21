using NebulaLicensingServer.Domain.Entities;

namespace NebulaLicensingServer.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    IQueryable<AuditLog> Query();
}
