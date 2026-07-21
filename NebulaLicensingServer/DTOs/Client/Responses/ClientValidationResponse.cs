namespace NebulaLicensingServer.DTOs.Client.Responses;

public sealed class ClientValidationResponse
{
    public bool IsValid { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTime ExpiresAt { get; init; }

    public int DaysRemaining { get; init; }

    public string Message { get; init; } = string.Empty;

    // =========================================
    // Signature
    // =========================================

    public string Signature { get; init; } = string.Empty;

    public string SignatureAlgorithm { get; init; } = "RSA-SHA256";

    public DateTime SignedAtUtc { get; init; }

    public DateTime OfflineGraceUntilUtc { get; init; }
}