using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NebulaLicensingServer.Domain.Entities;

namespace NebulaLicensingServer.Persistence.Configurations;

public sealed class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    private const int LicenseKeyMaxLength = 100;
    private const int MachineHashMaxLength = 100;
    private const int DeviceNameMaxLength = 200;
    private const int NotesMaxLength = 1000;

    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.ToTable("Licenses");

        builder.HasKey(license => license.Id);

        builder.Property(license => license.LicenseKey)
            .IsRequired()
            .HasMaxLength(LicenseKeyMaxLength);

        builder.Property(license => license.MachineHash)
            .HasMaxLength(MachineHashMaxLength);

        builder.Property(license => license.DeviceName)
            .HasMaxLength(DeviceNameMaxLength);

        builder.Property(license => license.Status)
            .IsRequired();

        builder.Property(license => license.CreatedAt)
            .IsRequired();

        builder.Property(license => license.ExpiresAt)
            .IsRequired();

        builder.Property(license => license.Notes)
            .HasMaxLength(NotesMaxLength);

        builder.HasIndex(license => license.LicenseKey)
            .IsUnique();

        builder.HasIndex(license => license.Status);
    }
}
