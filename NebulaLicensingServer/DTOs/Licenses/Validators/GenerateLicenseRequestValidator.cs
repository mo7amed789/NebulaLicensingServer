using FluentValidation;
using NebulaLicensingServer.DTOs.Licenses.Requests;

namespace NebulaLicensingServer.DTOs.Licenses.Validators;

public sealed class GenerateLicenseRequestValidator : AbstractValidator<GenerateLicenseRequest>
{
    public GenerateLicenseRequestValidator()
    {
        RuleFor(x => x.ValidMonths).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ValidDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ValidHours).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ValidMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x => x.ValidMonths > 0 || x.ValidDays > 0 || x.ValidHours > 0 || x.ValidMinutes > 0)
            .WithMessage("Please specify a valid validation duration.");
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
