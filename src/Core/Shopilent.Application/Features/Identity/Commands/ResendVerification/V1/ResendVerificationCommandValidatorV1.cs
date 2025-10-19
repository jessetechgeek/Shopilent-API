using FluentValidation;

namespace Shopilent.Application.Features.Identity.Commands.ResendVerification.V1;

internal sealed class ResendVerificationCommandValidatorV1 : AbstractValidator<ResendVerificationCommandV1>
{
    public ResendVerificationCommandValidatorV1()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");
    }
}