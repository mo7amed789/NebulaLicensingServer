using NebulaLicensingServer.Domain.Enums;

namespace NebulaLicensingServer.DTOs.Client.Responses;

public sealed class ClientHeartbeatResponse
{
    public DateTime CurrentServerTimeUtc { get; init; }

    public string Status { get; init; } = string.Empty;
}
