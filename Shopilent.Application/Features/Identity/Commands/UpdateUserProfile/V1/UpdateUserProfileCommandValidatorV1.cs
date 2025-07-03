using FluentValidation;

namespace Shopilent.Application.Features.Identity.Commands.UpdateUserProfile.V1;

internal sealed class UpdateUserProfileCommandValidatorV1 : AbstractValidator<UpdateUserProfileCommandV1>
{
    public UpdateUserProfileCommandValidatorV1()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.")
            .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("First name contains invalid characters.");

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.")
            .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Last name contains invalid characters.");

        RuleFor(v => v.MiddleName)
            .MaximumLength(50).WithMessage("Middle name must not exceed 50 characters.")
            .Matches(@"^[a-zA-Z\s\-'\.]*$").WithMessage("Middle name contains invalid characters.")
            .When(v => !string.IsNullOrEmpty(v.MiddleName));

        RuleFor(v => v.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number is not valid.")
            .When(v => !string.IsNullOrEmpty(v.Phone));
    }
}