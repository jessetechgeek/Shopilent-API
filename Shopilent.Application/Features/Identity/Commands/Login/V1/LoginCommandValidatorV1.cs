using FluentValidation;

namespace Shopilent.Application.Features.Identity.Commands.Login.V1;

internal sealed class LoginCommandValidatorV1 : AbstractValidator<LoginCommandV1>
{
    public LoginCommandValidatorV1()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}