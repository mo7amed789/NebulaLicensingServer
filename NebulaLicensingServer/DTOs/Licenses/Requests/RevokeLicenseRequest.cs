namespace NebulaLicensingServer.DTOs.Licenses.Requests;

public sealed class RevokeLicenseRequest
{
    public string LicenseKey { get; init; } = string.Empty;
}
