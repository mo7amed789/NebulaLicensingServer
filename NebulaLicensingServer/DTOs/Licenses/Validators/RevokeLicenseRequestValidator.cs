using FluentValidation;
using NebulaLicensingServer.DTOs.Licenses.Requests;

namespace NebulaLicensingServer.DTOs.Licenses.Validators;

public sealed class RevokeLicenseRequestValidator : AbstractValidator<RevokeLicenseRequest>
{
    public RevokeLicenseRequestValidator()
    {
        RuleFor(x => x.LicenseKey).NotEmpty().MaximumLength(100);
    }
}
