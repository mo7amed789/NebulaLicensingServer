namespace NebulaLicensingServer.DTOs.AuditLogs.Responses;

public sealed class AuditLogResponse
{
    public Guid Id { get; init; }

    public string Action { get; init; } = string.Empty;

    public string? Administrator { get; init; }

    public string? LicenseKey { get; init; }

    public string Result { get; init; } = string.Empty;

    public string? IpAddress { get; init; }

    public string? Details { get; init; }

    public DateTime CreatedAt { get; init; }
}
