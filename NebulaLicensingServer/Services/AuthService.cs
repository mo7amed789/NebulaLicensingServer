using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using NebulaLicensingServer.Common;
using NebulaLicensingServer.Constants;
using NebulaLicensingServer.Domain.Entities;
using NebulaLicensingServer.DTOs.Requests;
using NebulaLicensingServer.DTOs.Responses;
using NebulaLicensingServer.Interfaces;
using NebulaLicensingServer.Persistence;
using NebulaLicensingServer.Settings;

namespace NebulaLicensingServer.Services;

public sealed class AuthService(
    ApplicationDbContext context,
    IJwtService jwtService,
    IAuditLogService auditLogService,
    IOptions<JwtSettings> options,
    IHttpContextAccessor httpContextAccessor) : IAuthService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IJwtService _jwtService = jwtService;
    private readonly IAuditLogService _auditLogService = auditLogService;
    private readonly JwtSettings _settings = options.Value;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<LoginResponse>> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var admin = await _context.Admins.SingleOrDefaultAsync(x => x.Username == request.Username, cancellationToken);
        if (admin is null)
        {
            return Result<LoginResponse>.Failure(ErrorCodes.AuthInvalidCredentials, "Invalid credentials.");
        }

        if (admin.IsDisabled)
        {
            return Result<LoginResponse>.Failure(ErrorCodes.AuthInvalidCredentials, "Invalid credentials.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
        {
            return Result<LoginResponse>.Failure(ErrorCodes.AuthInvalidCredentials, "Invalid credentials.");
        }

        ArgumentNullException.ThrowIfNull(admin);
        var principal = CreatePrincipal(admin);
        var accessToken = _jwtService.GenerateAccessToken(principal);
        var refreshToken = _jwtService.GenerateRefreshToken();

        admin.RefreshTokenHash = HashRefreshToken(refreshToken);
        admin.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);
        admin.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            RefreshTokenExpiresAt = admin.RefreshTokenExpiresAt.Value
        };

        await _auditLogService.LogAsync("Administrator Login", admin.Username, null, "Success", GetIpAddress(), cancellationToken: cancellationToken);
        return Result<LoginResponse>.Success(response);
    }

    public async Task<Result<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result<RefreshTokenResponse>.Failure(ErrorCodes.AuthInvalidTokenPair, "Invalid token pair.");
        }

        var adminIdClaim = principal.FindFirstValue(Claims.Subject);
        if (!Guid.TryParse(adminIdClaim, out var adminId))
        {
            return Result<RefreshTokenResponse>.Failure(ErrorCodes.AuthInvalidTokenPair, "Invalid token pair.");
        }

        var admin = await _context.Admins.SingleOrDefaultAsync(x => x.Id == adminId, cancellationToken);
        if (admin is null || !IsRefreshTokenValid(admin, request.RefreshToken))
        {
            return Result<RefreshTokenResponse>.Failure(ErrorCodes.AuthInvalidTokenPair, "Invalid token pair.");
        }

        var accessToken = _jwtService.GenerateAccessToken(principal);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        admin.RefreshTokenHash = HashRefreshToken(newRefreshToken);
        admin.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);
        admin.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var response = new RefreshTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            RefreshTokenExpiresAt = admin.RefreshTokenExpiresAt.Value
        };

        return Result<RefreshTokenResponse>.Success(response);
    }

    private static ClaimsPrincipal CreatePrincipal(Admin admin)
    {
        var claims = new List<Claim>
        {
            new(Claims.Subject, admin.Id.ToString()),
            new(Claims.Name, admin.Username),
            new(Claims.Role, admin.Role),
            new(Claims.JwtId, Guid.NewGuid().ToString())
        };

        var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    private static bool IsRefreshTokenValid(Admin admin, string refreshToken)
    {
        if (admin.RefreshTokenHash is null || admin.RefreshTokenExpiresAt is null)
        {
            return false;
        }

        if (admin.RefreshTokenExpiresAt.Value <= DateTime.UtcNow)
        {
            return false;
        }

        var expected = Convert.FromBase64String(admin.RefreshTokenHash);
        var actual = Convert.FromBase64String(HashRefreshToken(refreshToken));
        return expected.Length == actual.Length && CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    private static string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(bytes);
    }

    private string? GetIpAddress() => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
