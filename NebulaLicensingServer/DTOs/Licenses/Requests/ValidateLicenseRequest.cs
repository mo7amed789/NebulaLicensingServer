namespace NebulaLicensingServer.DTOs.Licenses.Requests;

public sealed class ValidateLicenseRequest
{
    public string LicenseKey { get; init; } = string.Empty;

    public string MachineHash { get; init; } = string.Empty;
}
