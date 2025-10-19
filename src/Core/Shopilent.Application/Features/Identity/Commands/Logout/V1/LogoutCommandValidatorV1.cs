using FluentValidation;

namespace Shopilent.Application.Features.Identity.Commands.Logout.V1;

internal sealed class LogoutCommandValidatorV1 : AbstractValidator<LogoutCommandV1>
{
    public LogoutCommandValidatorV1()
    {
        RuleFor(v => v.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}