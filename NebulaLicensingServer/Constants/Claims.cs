using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NebulaLicensingServer.Constants;

public static class Claims
{
    public const string Subject = JwtRegisteredClaimNames.Sub;
    public const string Name = ClaimTypes.Name;
    public const string Role = ClaimTypes.Role;
    public const string JwtId = JwtRegisteredClaimNames.Jti;
}
