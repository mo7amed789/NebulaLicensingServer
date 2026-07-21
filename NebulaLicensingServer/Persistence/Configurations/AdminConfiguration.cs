using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NebulaLicensingServer.Domain.Entities;

namespace NebulaLicensingServer.Persistence.Configurations;

public sealed class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    private const int UsernameMaxLength = 50;
    private const int PasswordHashMaxLength = 100;
    private const int RefreshTokenHashMaxLength = 100;
    private const int RoleMaxLength = 50;

    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable("Admins");

        builder.HasKey(admin => admin.Id);

        builder.Property(admin => admin.Username)
            .IsRequired()
            .HasMaxLength(UsernameMaxLength);

        builder.Property(admin => admin.PasswordHash)
            .IsRequired()
            .HasMaxLength(PasswordHashMaxLength);

        builder.Property(admin => admin.IsDisabled)
            .IsRequired();

        builder.Property(admin => admin.RefreshTokenHash)
            .HasMaxLength(RefreshTokenHashMaxLength);

        builder.Property(admin => admin.RefreshTokenExpiresAt);

        builder.Property(admin => admin.Role)
            .IsRequired()
            .HasMaxLength(RoleMaxLength);

        builder.Property(admin => admin.CreatedAt)
            .IsRequired();

        builder.HasIndex(admin => admin.Username)
            .IsUnique();

        builder.HasIndex(admin => admin.IsDisabled);
    }
}
