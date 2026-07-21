using NebulaLicensingServer.Domain;

namespace NebulaLicensingServer.Domain.Entities;

public sealed class AuditLog : BaseEntity
{
    public string Action { get; set; } = string.Empty;

    public string? Administrator { get; set; }

    public string? LicenseKey { get; set; }

    public string Result { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public string? Details { get; set; }
}
