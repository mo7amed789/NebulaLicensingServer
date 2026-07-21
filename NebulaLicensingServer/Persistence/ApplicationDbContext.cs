using Microsoft.EntityFrameworkCore;
using NebulaLicensingServer.Domain.Entities;

namespace NebulaLicensingServer.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Admin> Admins => Set<Admin>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<License> Licenses => Set<License>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
