using NebulaLicensingServer.Common;
using NebulaLicensingServer.DTOs.Licenses.Requests;
using NebulaLicensingServer.DTOs.Licenses.Responses;

namespace NebulaLicensingServer.Interfaces;

public interface ILicenseService
{
    Task<Result<LicenseResponse>> GenerateLicenseAsync(GenerateLicenseRequest request, CancellationToken cancellationToken = default);

    Task<Result<LicenseResponse>> ActivateLicenseAsync(ActivateLicenseRequest request, CancellationToken cancellationToken = default);

    Task<Result<LicenseValidationResponse>> ValidateLicenseAsync(ValidateLicenseRequest request, CancellationToken cancellationToken = default);

    Task<Result<LicenseResponse>> ExtendLicenseAsync(ExtendLicenseRequest request, CancellationToken cancellationToken = default);

    Task<Result<LicenseResponse>> RevokeLicenseAsync(RevokeLicenseRequest request, CancellationToken cancellationToken = default);

    Task<Result<bool>> DeleteLicenseAsync(string licenseKey, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LicenseResponse>>> GetAllLicensesAsync(CancellationToken cancellationToken = default);

    Task<Result<LicenseResponse>> GetLicenseByKeyAsync(string licenseKey, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<LicenseResponse>>> SearchLicensesAsync(LicenseSearchRequest request, CancellationToken cancellationToken = default);

    Task<Result<LicenseResponse>> HeartbeatAsync(string licenseKey, string machineHash, CancellationToken cancellationToken = default);
}