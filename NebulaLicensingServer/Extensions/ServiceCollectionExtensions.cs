using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using NebulaLicensingServer.Interfaces;
using NebulaLicensingServer.Persistence;
using NebulaLicensingServer.Persistence.Repositories;
using NebulaLicensingServer.Services;
using NebulaLicensingServer.Settings;

namespace NebulaLicensingServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityFoundation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(settings =>
                !string.IsNullOrWhiteSpace(settings.Issuer) &&
                !string.IsNullOrWhiteSpace(settings.Audience) &&
                !string.IsNullOrWhiteSpace(settings.SecretKey) &&
                settings.AccessTokenExpirationMinutes > 0 &&
                settings.RefreshTokenExpirationDays > 0,
                "Jwt settings are invalid.");

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<IAdministratorService, AdministratorService>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAuditLogQueryService, AuditLogService>();
        services.AddHttpContextAccessor();
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Program>();

        return services;
    }

    public static IServiceCollection AddAuthenticationFoundation(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureJwtAuthentication(configuration);
        services.AddAuthorization();

        return services;
    }
}
