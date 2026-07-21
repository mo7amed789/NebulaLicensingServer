using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NebulaLicensingServer.Domain.Entities;

namespace NebulaLicensingServer.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Administrator).HasMaxLength(50);
        builder.Property(x => x.LicenseKey).HasMaxLength(100);
        builder.Property(x => x.Result).IsRequired().HasMaxLength(50);
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.Details).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.Action);
        builder.HasIndex(x => x.Administrator);
        builder.HasIndex(x => x.LicenseKey);
        builder.HasIndex(x => x.CreatedAt);
    }
}
