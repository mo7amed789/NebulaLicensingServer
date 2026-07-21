using Microsoft.EntityFrameworkCore;
using NebulaLicensingServer.Common;
using NebulaLicensingServer.Domain.Entities;
using NebulaLicensingServer.DTOs.AuditLogs.Requests;
using NebulaLicensingServer.DTOs.AuditLogs.Responses;
using NebulaLicensingServer.DTOs.Licenses.Responses;
using NebulaLicensingServer.Interfaces;
using NebulaLicensingServer.Persistence;
using Serilog;

namespace NebulaLicensingServer.Services;

public sealed class AuditLogService(IAuditLogRepository repository, ApplicationDbContext context) : IAuditLogService, IAuditLogQueryService
{
    private readonly IAuditLogRepository _repository = repository;
    private readonly ApplicationDbContext _context = context;

    public async Task LogAsync(string action, string? administrator, string? licenseKey, string result, string? ipAddress = null, string? details = null, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = action,
            Administrator = administrator,
            LicenseKey = licenseKey,
            Result = result,
            IpAddress = ipAddress,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        Log.Information("Audit {Action} {Administrator} {LicenseKey} {Result} {IpAddress}", action, administrator, licenseKey, result, ipAddress);
    }

    public async Task<Result<PagedResult<AuditLogResponse>>> SearchAsync(AuditLogSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = _repository.Query();

        if (!string.IsNullOrWhiteSpace(request.User))
        {
            query = query.Where(x => x.Administrator != null && x.Administrator.Contains(request.User));
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(x => x.Action.Contains(request.Action));
        }

        if (!string.IsNullOrWhiteSpace(request.License))
        {
            query = query.Where(x => x.LicenseKey != null && x.LicenseKey.Contains(request.License));
        }

        if (request.From is not null)
        {
            query = query.Where(x => x.CreatedAt >= request.From.Value);
        }

        if (request.To is not null)
        {
            query = query.Where(x => x.CreatedAt <= request.To.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.IpAddress))
        {
            query = query.Where(x => x.IpAddress != null && x.IpAddress.Contains(request.IpAddress));
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditLogResponse
            {
                Id = x.Id,
                Action = x.Action,
                Administrator = x.Administrator,
                LicenseKey = x.LicenseKey,
                Result = x.Result,
                IpAddress = x.IpAddress,
                Details = x.Details,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<AuditLogResponse>>.Success(new PagedResult<AuditLogResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }
}
