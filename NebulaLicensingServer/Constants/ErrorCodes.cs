namespace NebulaLicensingServer.Constants;

public static class ErrorCodes
{
    public const string AuthInvalidCredentials = "AUTH-001";
    public const string AuthInvalidTokenPair = "AUTH-002";

    public const string LicenseNotFound = "LIC-001";
    public const string LicenseConflict = "LIC-002";
    public const string LicenseValidationFailed = "LIC-003";

    public const string ValidationFailed = "VAL-001";
}
