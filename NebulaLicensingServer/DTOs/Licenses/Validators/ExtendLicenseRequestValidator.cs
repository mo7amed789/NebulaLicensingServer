using FluentValidation;
using NebulaLicensingServer.DTOs.Licenses.Requests;

namespace NebulaLicensingServer.DTOs.Licenses.Validators;

public sealed class ExtendLicenseRequestValidator : AbstractValidator<ExtendLicenseRequest>
{
    public ExtendLicenseRequestValidator()
    {
        RuleFor(x => x.LicenseKey).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NewExpirationDate).GreaterThan(DateTime.UtcNow);
    }
}
