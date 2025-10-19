using FluentValidation;

namespace Shopilent.Application.Features.Identity.Commands.ResetPassword.V1;

internal sealed class ResetPasswordCommandValidatorV1 : AbstractValidator<ResetPasswordCommandV1>
{
    public ResetPasswordCommandValidatorV1()
    {
        RuleFor(v => v.Token)
            .NotEmpty().WithMessage("Token is required.");

        RuleFor(v => v.NewPassword)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(v => v.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(v => v.NewPassword).WithMessage("Passwords do not match.");
    }
}