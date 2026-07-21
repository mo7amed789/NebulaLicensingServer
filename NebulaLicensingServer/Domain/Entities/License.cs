using NebulaLicensingServer.Domain;
using NebulaLicensingServer.Domain.Enums;

namespace NebulaLicensingServer.Domain.Entities;

public sealed class License : BaseEntity
{
    public string LicenseKey { get; set; } = string.Empty;

    public string? MachineHash { get; set; }

    public string? DeviceName { get; set; }

    public LicenseStatus Status { get; set; }

    public DateTime? ActivatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? LastValidation { get; set; }

    public DateTime? LastSeen { get; set; }

    public string? Notes { get; set; }
}
