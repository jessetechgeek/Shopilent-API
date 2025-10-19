using FluentValidation;

namespace Shopilent.Application.Features.Identity.Commands.ForgotPassword.V1;

internal sealed class ForgotPasswordCommandValidatorV1 : AbstractValidator<ForgotPasswordCommandV1>
{
    public ForgotPasswordCommandValidatorV1()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");
    }
}