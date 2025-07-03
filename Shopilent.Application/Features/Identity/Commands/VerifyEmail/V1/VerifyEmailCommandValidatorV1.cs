using FluentValidation;

namespace Shopilent.Application.Features.Identity.Commands.VerifyEmail.V1;

internal sealed class VerifyEmailCommandValidatorV1 : AbstractValidator<VerifyEmailCommandV1>
{
    public VerifyEmailCommandValidatorV1()
    {
        RuleFor(v => v.Token)
            .NotEmpty().WithMessage("Verification token is required.");
    }
}