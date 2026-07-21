using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using NebulaLicensingServer.Settings;

namespace NebulaLicensingServer.Services;

public sealed class LicenseSigningService
{
    private readonly LicenseSigningOptions _options;

    public LicenseSigningService(
        IOptions<LicenseSigningOptions> options)
    {
        _options = options.Value;
    }

    public LicenseSignature Sign(
        string licenseKey,
        string machineHash,
        string deviceName,
        string nebulaVersion,
        string serverId,
        DateTime expiresAt,
        bool activated)
    {
        var signedAt = DateTime.UtcNow;

        var graceUntil = signedAt.AddDays(_options.OfflineGraceDays);

        // 1. هنا يتم تجميع البيانات لتكوين الـ Payload
        var payload = string.Join("|",
            licenseKey,
            machineHash,
            deviceName,
            nebulaVersion,
            serverId,
            expiresAt.ToUniversalTime().ToString("O"),
            signedAt.ToUniversalTime().ToString("O"),
            graceUntil.ToUniversalTime().ToString("O"),
            activated);

        // =========================================================
        // 2. كود التشخيص الذي طلبته (تمت إضافته هنا)
        // =========================================================
        Console.WriteLine("========== SIGN PAYLOAD ==========");
        Console.WriteLine(payload);
        Console.WriteLine("==================================");
        // =========================================================

        using var rsa = RSA.Create();

        var pem = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Keys", "private.pem"));

        rsa.ImportFromPem(pem);

        var signature = rsa.SignData(
            Encoding.UTF8.GetBytes(payload),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return new LicenseSignature
        {
            Signature = Convert.ToBase64String(signature),
            SignatureAlgorithm = "RSA-SHA256",
            SignedAtUtc = signedAt,
            OfflineGraceUntilUtc = graceUntil
        };
    }
}

public sealed class LicenseSignature
{
    public string Signature { get; init; } = string.Empty;

    public string SignatureAlgorithm { get; init; } = "RSA-SHA256";

    public DateTime SignedAtUtc { get; init; }

    public DateTime OfflineGraceUntilUtc { get; init; }
}