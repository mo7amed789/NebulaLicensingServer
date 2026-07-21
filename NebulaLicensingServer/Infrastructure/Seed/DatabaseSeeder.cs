using Microsoft.EntityFrameworkCore;
using NebulaLicensingServer.Domain.Entities;
using NebulaLicensingServer.Persistence;

namespace NebulaLicensingServer.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, IHostEnvironment environment)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync();

        if (!await context.Admins.AnyAsync())
        {
            var initialPassword = configuration["Admin:InitialPassword"];
            if (string.IsNullOrWhiteSpace(initialPassword))
            {
                if (environment.IsDevelopment())
                {
                    initialPassword = "admin123";
                }
                else
                {
                    throw new InvalidOperationException("Missing Admin:InitialPassword configuration.");
                }
            }

            context.Admins.Add(new Admin
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(initialPassword),
                Role = "Administrator",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
    }
}
