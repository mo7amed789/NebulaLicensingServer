using NebulaLicensingServer.Domain.Enums;

namespace NebulaLicensingServer.DTOs.Licenses.Responses;

public sealed class LicenseResponse
{
    public Guid Id { get; init; }

    public string LicenseKey { get; init; } = string.Empty;

    public string? MachineHash { get; init; }

    public string? DeviceName { get; init; }

    public LicenseStatus Status { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? ActivatedAt { get; init; }

    // تم تعديلها لتصبح Nullable لأن التاريخ لا يتم احتسابه إلا عند تفعيل العميل للترخيص
    public DateTime? ExpiresAt { get; init; }

    public DateTime? LastValidation { get; init; }

    public string? Notes { get; init; }
}