using System.Security.Claims;

namespace NebulaLicensingServer.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(ClaimsPrincipal principal);

    string GenerateRefreshToken();

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    bool ValidateToken(string token);
}
