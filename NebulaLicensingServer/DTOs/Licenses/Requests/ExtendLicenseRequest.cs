namespace NebulaLicensingServer.DTOs.Licenses.Requests;

public sealed class ExtendLicenseRequest
{
    public string LicenseKey { get; init; } = string.Empty;

    public DateTime NewExpirationDate { get; init; }
}
