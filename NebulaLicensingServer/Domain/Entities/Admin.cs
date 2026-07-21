using NebulaLicensingServer.Domain;

namespace NebulaLicensingServer.Domain.Entities;

public sealed class Admin : BaseEntity
{
    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsDisabled { get; set; }

    public string? RefreshTokenHash { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    public string Role { get; set; } = string.Empty;
}
