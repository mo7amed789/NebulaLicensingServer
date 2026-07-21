using FluentValidation;
using NebulaLicensingServer.DTOs.Requests;

namespace NebulaLicensingServer.DTOs.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(200);
    }
}
