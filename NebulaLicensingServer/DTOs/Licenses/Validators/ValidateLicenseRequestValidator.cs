using FluentValidation;
using NebulaLicensingServer.DTOs.Licenses.Requests;

namespace NebulaLicensingServer.DTOs.Licenses.Validators;

public sealed class ValidateLicenseRequestValidator : AbstractValidator<ValidateLicenseRequest>
{
    public ValidateLicenseRequestValidator()
    {
        RuleFor(x => x.LicenseKey).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MachineHash).NotEmpty().MaximumLength(100);
    }
}
