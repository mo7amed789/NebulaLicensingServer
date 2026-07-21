namespace NebulaLicensingServer.ViewModels.Dashboard;

public sealed class LicenseListItemViewModel
{
    public Guid Id { get; set; }

    public string LicenseKey { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? DeviceName { get; set; }

    public string? MachineHash { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? ActivatedAt { get; set; }

    public DateTime? LastValidation { get; set; }

    public string? Notes { get; set; }

    public bool IsExpired { get; set; }
}
