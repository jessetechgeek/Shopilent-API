using FluentValidation;

namespace Shopilent.Application.Features.Identity.Commands.ChangePassword.V1;

internal sealed class ChangePasswordCommandValidatorV1 : AbstractValidator<ChangePasswordCommandV1>
{
    public ChangePasswordCommandValidatorV1()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(v => v.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(v => v.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.")
            .NotEqual(v => v.CurrentPassword).WithMessage("New password must be different from current password.");

        RuleFor(v => v.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(v => v.NewPassword).WithMessage("Passwords do not match.");
    }
}