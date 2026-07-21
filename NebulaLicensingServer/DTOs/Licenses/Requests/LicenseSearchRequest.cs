using NebulaLicensingServer.Domain.Enums;

namespace NebulaLicensingServer.DTOs.Licenses.Requests;

public sealed class LicenseSearchRequest
{
    public string? LicenseKey { get; init; }

    public LicenseStatus? Status { get; init; }

    public string? MachineHash { get; init; }

    public int? Page { get; init; }

    public int? PageSize { get; init; }
}
