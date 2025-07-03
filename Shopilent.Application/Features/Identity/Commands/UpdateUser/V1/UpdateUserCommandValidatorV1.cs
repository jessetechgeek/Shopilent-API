using FluentValidation;

namespace Shopilent.Application.Features.Identity.Commands.UpdateUser.V1;

internal sealed class UpdateUserCommandValidatorV1 : AbstractValidator<UpdateUserCommandV1>
{
    public UpdateUserCommandValidatorV1()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(v => v.MiddleName)
            .MaximumLength(50).WithMessage("Middle name cannot exceed 50 characters.")
            .When(v => !string.IsNullOrEmpty(v.MiddleName));

        RuleFor(v => v.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number format is invalid.")
            .When(v => !string.IsNullOrEmpty(v.Phone));
    }
}