using FluentValidation;

namespace Shopilent.Application.Features.Identity.Commands.RefreshToken.V1;

internal sealed class RefreshTokenCommandValidatorV1 : AbstractValidator<RefreshTokenCommandV1>
{
    public RefreshTokenCommandValidatorV1()
    {
        RuleFor(v => v.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}