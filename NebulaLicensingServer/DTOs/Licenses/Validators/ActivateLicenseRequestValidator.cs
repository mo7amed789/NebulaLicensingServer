using FluentValidation;
using NebulaLicensingServer.DTOs.Licenses.Requests;

namespace NebulaLicensingServer.DTOs.Licenses.Validators;

public sealed class ActivateLicenseRequestValidator : AbstractValidator<ActivateLicenseRequest>
{
    public ActivateLicenseRequestValidator()
    {
        RuleFor(x => x.LicenseKey).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MachineHash).MaximumLength(100);
        RuleFor(x => x.DeviceName).MaximumLength(200);
    }
}
