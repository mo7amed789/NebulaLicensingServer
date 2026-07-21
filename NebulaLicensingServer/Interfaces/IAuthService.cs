using NebulaLicensingServer.Common;
using NebulaLicensingServer.DTOs.Requests;
using NebulaLicensingServer.DTOs.Responses;

namespace NebulaLicensingServer.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponse>> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<Result<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}
