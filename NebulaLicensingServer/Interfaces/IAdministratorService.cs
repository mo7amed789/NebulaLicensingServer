using NebulaLicensingServer.Common;
using NebulaLicensingServer.DTOs.Administrators.Requests;
using NebulaLicensingServer.DTOs.Administrators.Responses;

namespace NebulaLicensingServer.Interfaces;

public interface IAdministratorService
{
    Task<Result<IReadOnlyList<AdministratorResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<AdministratorResponse>> CreateAsync(AdministratorCreateRequest request, CancellationToken cancellationToken = default);
    Task<Result<AdministratorResponse>> ResetPasswordAsync(Guid id, string password, CancellationToken cancellationToken = default);
    Task<Result<AdministratorResponse>> SetDisabledAsync(Guid id, bool isDisabled, CancellationToken cancellationToken = default);
    Task<Result<AdministratorResponse>> ChangeRoleAsync(Guid id, string role, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
