using FluentValidation;
using NebulaLicensingServer.DTOs.Client.Requests;

namespace NebulaLicensingServer.DTOs.Client.Validators;

public sealed class ClientValidateRequestValidator : AbstractValidator<ClientValidateRequest>
{
    public ClientValidateRequestValidator()
    {
        RuleFor(x => x.LicenseKey).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MachineHash).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NebulaVersion).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ServerId).NotEmpty().MaximumLength(100);
    }
}
