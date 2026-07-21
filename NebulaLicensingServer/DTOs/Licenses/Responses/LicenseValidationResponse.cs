namespace NebulaLicensingServer.DTOs.Licenses.Responses;

public sealed class LicenseValidationResponse
{
    public bool IsValid { get; init; }

    public string? Message { get; init; }

    // ===============================
    // Signature Information
    // ===============================

    public string Signature { get; init; } = string.Empty;

    public string SignatureAlgorithm { get; init; } = "RSA-SHA256";

    public DateTime SignedAtUtc { get; init; }

    public DateTime OfflineGraceUntilUtc { get; init; }
}