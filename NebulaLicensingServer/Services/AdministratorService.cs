using Microsoft.EntityFrameworkCore;
using NebulaLicensingServer.Common;
using NebulaLicensingServer.Domain.Entities;
using NebulaLicensingServer.DTOs.Administrators.Requests;
using NebulaLicensingServer.DTOs.Administrators.Responses;
using NebulaLicensingServer.Interfaces;
using NebulaLicensingServer.Persistence;

namespace NebulaLicensingServer.Services;

public sealed class AdministratorService(ApplicationDbContext context, IAuditLogService auditLogService, IHttpContextAccessor httpContextAccessor) : IAdministratorService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IAuditLogService _auditLogService = auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<IReadOnlyList<AdministratorResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var admins = await _context.Admins.AsNoTracking().OrderBy(x => x.Username).Select(x => new AdministratorResponse
        {
            Id = x.Id,
            Username = x.Username,
            Role = x.Role,
            IsDisabled = x.IsDisabled
        }).ToListAsync(cancellationToken);
        return Result<IReadOnlyList<AdministratorResponse>>.Success(admins);
    }

    public async Task<Result<AdministratorResponse>> CreateAsync(AdministratorCreateRequest request, CancellationToken cancellationToken = default)
    {
        var admin = new Admin
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };
        _context.Admins.Add(admin);
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync("Administrator Created", CurrentUser(), admin.Username, "Success", CurrentIp(), request.Role, cancellationToken);
        return Result<AdministratorResponse>.Success(ToResponse(admin));
    }

    public async Task<Result<AdministratorResponse>> ResetPasswordAsync(Guid id, string password, CancellationToken cancellationToken = default)
    {
        var admin = await _context.Admins.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (admin is null) return Result<AdministratorResponse>.Failure("ADM-404", "Administrator not found.");
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        admin.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync("Administrator Password Reset", CurrentUser(), admin.Username, "Success", CurrentIp(), cancellationToken: cancellationToken);
        return Result<AdministratorResponse>.Success(ToResponse(admin));
    }

    public async Task<Result<AdministratorResponse>> SetDisabledAsync(Guid id, bool isDisabled, CancellationToken cancellationToken = default)
    {
        var admin = await _context.Admins.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (admin is null) return Result<AdministratorResponse>.Failure("ADM-404", "Administrator not found.");
        admin.IsDisabled = isDisabled;
        admin.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync(isDisabled ? "Administrator Disabled" : "Administrator Enabled", CurrentUser(), admin.Username, "Success", CurrentIp(), cancellationToken: cancellationToken);
        return Result<AdministratorResponse>.Success(ToResponse(admin));
    }

    public async Task<Result<AdministratorResponse>> ChangeRoleAsync(Guid id, string role, CancellationToken cancellationToken = default)
    {
        var admin = await _context.Admins.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (admin is null) return Result<AdministratorResponse>.Failure("ADM-404", "Administrator not found.");
        admin.Role = role;
        admin.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync("Administrator Role Changed", CurrentUser(), admin.Username, "Success", CurrentIp(), role, cancellationToken);
        return Result<AdministratorResponse>.Success(ToResponse(admin));
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var total = await _context.Admins.CountAsync(cancellationToken);
        if (total <= 1)
        {
            return Result<bool>.Failure("ADM-409", "Cannot delete the last administrator.");
        }
        var admin = await _context.Admins.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (admin is null) return Result<bool>.Failure("ADM-404", "Administrator not found.");
        _context.Admins.Remove(admin);
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync("Administrator Deleted", CurrentUser(), admin.Username, "Success", CurrentIp(), cancellationToken: cancellationToken);
        return Result<bool>.Success(true);
    }

    private AdministratorResponse ToResponse(Admin admin) => new() { Id = admin.Id, Username = admin.Username, Role = admin.Role, IsDisabled = admin.IsDisabled };
    private string? CurrentUser() => _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    private string? CurrentIp() => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
