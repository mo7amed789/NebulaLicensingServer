namespace NebulaLicensingServer.DTOs.Licenses.Requests;

public sealed class ActivateLicenseRequest
{
    public string LicenseKey { get; init; } = string.Empty;

    public string? MachineHash { get; init; }

    public string? DeviceName { get; init; }

    // نفس القيم القادمة من NebulaServer
    public string? NebulaVersion { get; init; }

    public string? ServerId { get; init; }
}