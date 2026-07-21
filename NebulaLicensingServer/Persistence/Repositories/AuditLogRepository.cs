using Microsoft.EntityFrameworkCore;
using NebulaLicensingServer.Domain.Entities;
using NebulaLicensingServer.Interfaces;

namespace NebulaLicensingServer.Persistence.Repositories;

public sealed class AuditLogRepository(ApplicationDbContext context) : IAuditLogRepository
{
    private readonly ApplicationDbContext _context = context;

    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        _context.AuditLogs.Add(auditLog);
        return Task.CompletedTask;
    }

    public IQueryable<AuditLog> Query() => _context.AuditLogs.AsNoTracking();
}
