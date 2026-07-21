namespace NebulaLicensingServer.DTOs.Client.Requests;

public sealed class ClientHeartbeatRequest
{
    public string LicenseKey { get; init; } = string.Empty;

    public string MachineHash { get; init; } = string.Empty;

    public string NebulaVersion { get; init; } = string.Empty;

    public string ServerId { get; init; } = string.Empty;

    public DateTime CurrentTimeUtc { get; init; }
}
