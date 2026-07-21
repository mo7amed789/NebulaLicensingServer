namespace NebulaLicensingServer.DTOs.AuditLogs.Requests;

public sealed class AuditLogSearchRequest
{
    public string? User { get; init; }

    public string? Action { get; init; }

    public string? License { get; init; }

    public DateTime? From { get; init; }

    public DateTime? To { get; init; }

    public string? IpAddress { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
