namespace NebulaLicensingServer.DTOs.Client.Responses;

public sealed class ClientActivationResponse
{
    public bool Success { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public string LicenseKey { get; set; } = string.Empty;

    public string MachineName { get; set; } = string.Empty;

    // ===========================
    // Signature
    // ===========================

    public string Signature { get; set; } = string.Empty;

    public string SignatureAlgorithm { get; set; } = "RSA-SHA256";

    public DateTime SignedAtUtc { get; set; }

    public DateTime OfflineGraceUntilUtc { get; set; }
}