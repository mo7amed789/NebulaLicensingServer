namespace NebulaLicensingServer.DTOs.Administrators.Requests;

public sealed class AdministratorCreateRequest
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Role { get; init; } = "Administrator";
}
