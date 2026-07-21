namespace NebulaLicensingServer.DTOs.Administrators.Responses;

public sealed class AdministratorResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsDisabled { get; init; }
}
