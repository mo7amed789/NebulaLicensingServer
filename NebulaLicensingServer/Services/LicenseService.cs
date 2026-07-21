using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NebulaLicensingServer.Common;
using NebulaLicensingServer.Constants;
using NebulaLicensingServer.Domain.Entities;
using NebulaLicensingServer.Domain.Enums;
using NebulaLicensingServer.DTOs.Licenses.Requests;
using NebulaLicensingServer.DTOs.Licenses.Responses;
using NebulaLicensingServer.Interfaces;
using NebulaLicensingServer.Persistence;

namespace NebulaLicensingServer.Services;

public sealed class LicenseService(ApplicationDbContext context, IAuditLogService auditLogService, IHttpContextAccessor httpContextAccessor) : ILicenseService
{
    private const string Prefix = "NBL";
    private const int SegmentCount = 4;
    private const int SegmentLength = 4;

    private readonly ApplicationDbContext _context = context;
    private readonly IAuditLogService _auditLogService = auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<LicenseResponse>> GenerateLicenseAsync(GenerateLicenseRequest request, CancellationToken cancellationToken = default)
    {
        var licenseKey = await GenerateUniqueLicenseKeyAsync(cancellationToken);
        var now = DateTime.UtcNow;

        // العد يبدأ من لحظة الإنشاء (now) مباشرة، مش من وقت التفعيل
        // ده اللي كان بيسبب مشكلة "01 Jan 0001" لأن request.ExpiresAt كانت دايمًا default
        var expiresAt = now
            .AddMonths(request.ValidMonths)
            .AddDays(request.ValidDays)
            .AddHours(request.ValidHours)
            .AddMinutes(request.ValidMinutes);

        var license = new License
        {
            Id = Guid.NewGuid(),
            LicenseKey = licenseKey,
            Status = LicenseStatus.Generated,
            CreatedAt = now,
            ExpiresAt = expiresAt,
            Notes = request.Notes
        };

        _context.Licenses.Add(license);
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync("License Generated", CurrentUser(), license.LicenseKey, "Success", CurrentIp(), cancellationToken: cancellationToken);

        return Result<LicenseResponse>.Success(ToResponse(license));
    }

    public async Task<Result<LicenseResponse>> ActivateLicenseAsync(ActivateLicenseRequest request, CancellationToken cancellationToken = default)
    {
        var license = await _context.Licenses.SingleOrDefaultAsync(x => x.LicenseKey == request.LicenseKey, cancellationToken);
        if (license is null)
        {
            return Result<LicenseResponse>.Failure(ErrorCodes.LicenseNotFound, "License not found.");
        }

        if (license.Status == LicenseStatus.Revoked)
        {
            return Result<LicenseResponse>.Failure(ErrorCodes.LicenseConflict, "License is revoked.");
        }

        if (license.ExpiresAt <= DateTime.UtcNow)
        {
            license.Status = LicenseStatus.Expired;
            await _context.SaveChangesAsync(cancellationToken);
            return Result<LicenseResponse>.Failure(ErrorCodes.LicenseConflict, "License is expired.");
        }

        if (string.IsNullOrWhiteSpace(license.MachineHash))
        {
            license.MachineHash = request.MachineHash;
            license.DeviceName = request.DeviceName;
            license.ActivatedAt = DateTime.UtcNow;
            license.Status = LicenseStatus.Activated;
            // ملاحظة: مبنعملش تعديل على ExpiresAt هنا - العدّ بدأ من وقت الإنشاء ومستمر بدون تغيير
            await _context.SaveChangesAsync(cancellationToken);
            await _auditLogService.LogAsync("License Activated", CurrentUser(), license.LicenseKey, "Success", CurrentIp(), cancellationToken: cancellationToken);
            return Result<LicenseResponse>.Success(ToResponse(license));
        }

        if (!AreMachineHashesEqual(license.MachineHash, request.MachineHash))
        {
            return Result<LicenseResponse>.Failure(ErrorCodes.LicenseConflict, "License already belongs to another machine.");
        }

        return Result<LicenseResponse>.Success(ToResponse(license));
    }

    public async Task<Result<LicenseValidationResponse>> ValidateLicenseAsync(ValidateLicenseRequest request, CancellationToken cancellationToken = default)
    {
        var license = await _context.Licenses.SingleOrDefaultAsync(x => x.LicenseKey == request.LicenseKey, cancellationToken);
        if (license is null)
        {
            return Result<LicenseValidationResponse>.Success(new LicenseValidationResponse
            {
                IsValid = false,
                Message = "License not found."
            });
        }

        if (license.Status == LicenseStatus.Revoked)
        {
            return Result<LicenseValidationResponse>.Success(new LicenseValidationResponse
            {
                IsValid = false,
                Message = "License is revoked."
            });
        }

        if (license.ExpiresAt <= DateTime.UtcNow)
        {
            license.Status = LicenseStatus.Expired;
            await _context.SaveChangesAsync(cancellationToken);
            return Result<LicenseValidationResponse>.Success(new LicenseValidationResponse
            {
                IsValid = false,
                Message = "License is expired."
            });
        }

        if (string.IsNullOrWhiteSpace(license.MachineHash) || !AreMachineHashesEqual(license.MachineHash, request.MachineHash))
        {
            return Result<LicenseValidationResponse>.Success(new LicenseValidationResponse
            {
                IsValid = false,
                Message = "Machine hash does not match."
            });
        }

        license.LastValidation = DateTime.UtcNow;
        license.LastSeen = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync("License Validated", CurrentUser(), license.LicenseKey, "Success", CurrentIp(), cancellationToken: cancellationToken);

        return Result<LicenseValidationResponse>.Success(new LicenseValidationResponse
        {
            IsValid = true,
            Message = "License is valid."
        });
    }

    public async Task<Result<LicenseResponse>> ExtendLicenseAsync(ExtendLicenseRequest request, CancellationToken cancellationToken = default)
    {
        var license = await _context.Licenses.SingleOrDefaultAsync(x => x.LicenseKey == request.LicenseKey, cancellationToken);
        if (license is null)
        {
            return Result<LicenseResponse>.Failure(ErrorCodes.LicenseNotFound, "License not found.");
        }

        if (license.Status == LicenseStatus.Revoked)
        {
            return Result<LicenseResponse>.Failure(ErrorCodes.LicenseConflict, "Cannot extend revoked license.");
        }

        if (request.NewExpirationDate <= license.ExpiresAt)
        {
            return Result<LicenseResponse>.Failure(ErrorCodes.LicenseConflict, "Expiration date must move forward.");
        }

        license.ExpiresAt = request.NewExpirationDate;
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync("License Extended", CurrentUser(), license.LicenseKey, "Success", CurrentIp(), cancellationToken: cancellationToken);

        return Result<LicenseResponse>.Success(ToResponse(license));
    }

    public async Task<Result<LicenseResponse>> RevokeLicenseAsync(RevokeLicenseRequest request, CancellationToken cancellationToken = default)
    {
        var license = await _context.Licenses.SingleOrDefaultAsync(x => x.LicenseKey == request.LicenseKey, cancellationToken);
        if (license is null)
        {
            return Result<LicenseResponse>.Failure(ErrorCodes.LicenseNotFound, "License not found.");
        }

        license.Status = LicenseStatus.Revoked;
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync("License Revoked", CurrentUser(), license.LicenseKey, "Success", CurrentIp(), cancellationToken: cancellationToken);

        return Result<LicenseResponse>.Success(ToResponse(license));
    }

    public async Task<Result<bool>> DeleteLicenseAsync(string licenseKey, CancellationToken cancellationToken = default)
    {
        var license = await _context.Licenses.SingleOrDefaultAsync(x => x.LicenseKey == licenseKey, cancellationToken);
        if (license is null)
        {
            return Result<bool>.Failure(ErrorCodes.LicenseNotFound, "License not found.");
        }

        _context.Licenses.Remove(license);
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync("License Deleted", CurrentUser(), licenseKey, "Success", CurrentIp(), cancellationToken: cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<IReadOnlyList<LicenseResponse>>> GetAllLicensesAsync(CancellationToken cancellationToken = default)
    {
        var licenses = await _context.Licenses
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(ToResponseExpression())
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<LicenseResponse>>.Success(licenses);
    }

    public async Task<Result<PagedResult<LicenseResponse>>> SearchLicensesAsync(LicenseSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Licenses.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.LicenseKey))
        {
            query = query.Where(x => EF.Functions.Like(x.LicenseKey, $"%{request.LicenseKey}%"));
        }

        if (request.Status is not null)
        {
            query = query.Where(x => x.Status == request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.MachineHash))
        {
            query = query.Where(x => x.MachineHash == request.MachineHash);
        }

        var page = Math.Max(1, request.Page.GetValueOrDefault(1));
        var pageSize = Math.Clamp(request.PageSize.GetValueOrDefault(20), 1, 100);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToResponseExpression())
            .ToListAsync(cancellationToken);

        return Result<PagedResult<LicenseResponse>>.Success(new PagedResult<LicenseResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<LicenseResponse>> GetLicenseByKeyAsync(string licenseKey, CancellationToken cancellationToken = default)
    {
        var license = await _context.Licenses.AsNoTracking()
            .SingleOrDefaultAsync(x => x.LicenseKey == licenseKey, cancellationToken);

        return license is null
            ? Result<LicenseResponse>.Failure(ErrorCodes.LicenseNotFound, "License not found.")
            : Result<LicenseResponse>.Success(ToResponse(license));
    }

    public async Task<Result<LicenseResponse>> HeartbeatAsync(string licenseKey, string machineHash, CancellationToken cancellationToken = default)
    {
        var license = await _context.Licenses.SingleOrDefaultAsync(x => x.LicenseKey == licenseKey, cancellationToken);
        if (license is null)
        {
            return Result<LicenseResponse>.Failure(ErrorCodes.LicenseNotFound, "License not found.");
        }

        if (license.Status == LicenseStatus.Revoked)
        {
            return Result<LicenseResponse>.Failure(ErrorCodes.LicenseConflict, "License is revoked.");
        }

        if (license.ExpiresAt <= DateTime.UtcNow)
        {
            license.Status = LicenseStatus.Expired;
            await _context.SaveChangesAsync(cancellationToken);
            return Result<LicenseResponse>.Failure(ErrorCodes.LicenseConflict, "License is expired.");
        }

        if (string.IsNullOrWhiteSpace(license.MachineHash) || !AreMachineHashesEqual(license.MachineHash, machineHash))
        {
            return Result<LicenseResponse>.Failure(ErrorCodes.LicenseConflict, "Machine hash does not match.");
        }

        license.LastValidation = DateTime.UtcNow;
        license.LastSeen = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        await _auditLogService.LogAsync("Heartbeat", CurrentUser(), license.LicenseKey, "Success", CurrentIp(), cancellationToken: cancellationToken);

        return Result<LicenseResponse>.Success(ToResponse(license));
    }

    private static bool AreMachineHashesEqual(string expected, string? actual)
    {
        if (string.IsNullOrWhiteSpace(actual))
        {
            return false;
        }

        var left = Encoding.UTF8.GetBytes(expected);
        var right = Encoding.UTF8.GetBytes(actual);
        return left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);
    }

    private async Task<string> GenerateUniqueLicenseKeyAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var candidate = CreateLicenseKey();
            var exists = await _context.Licenses.AnyAsync(x => x.LicenseKey == candidate, cancellationToken);

            if (!exists)
            {
                return candidate;
            }
        }
    }

    private static string CreateLicenseKey()
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var key = new char[Prefix.Length + 1 + (SegmentCount * SegmentLength) + (SegmentCount - 1)];
        Prefix.AsSpan().CopyTo(key);
        key[Prefix.Length] = '-';

        var index = Prefix.Length + 1;
        for (var segment = 0; segment < SegmentCount; segment++)
        {
            for (var i = 0; i < SegmentLength; i++)
            {
                key[index++] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
            }

            if (segment < SegmentCount - 1)
            {
                key[index++] = '-';
            }
        }

        return new string(key);
    }

    private static LicenseResponse ToResponse(License license) => new()
    {
        Id = license.Id,
        LicenseKey = license.LicenseKey,
        MachineHash = license.MachineHash,
        DeviceName = license.DeviceName,
        Status = license.Status,
        CreatedAt = license.CreatedAt,
        ActivatedAt = license.ActivatedAt,
        ExpiresAt = license.ExpiresAt,
        LastValidation = license.LastValidation,
        Notes = license.Notes
    };

    private static Expression<Func<License, LicenseResponse>> ToResponseExpression() => license => new LicenseResponse
    {
        Id = license.Id,
        LicenseKey = license.LicenseKey,
        MachineHash = license.MachineHash,
        DeviceName = license.DeviceName,
        Status = license.Status,
        CreatedAt = license.CreatedAt,
        ActivatedAt = license.ActivatedAt,
        ExpiresAt = license.ExpiresAt,
        LastValidation = license.LastValidation,
        Notes = license.Notes
    };

    private string? CurrentUser() => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    private string? CurrentIp() => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}