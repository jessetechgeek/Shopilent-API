using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Users.UpdateProfile.V1;

public class UpdateUserProfileRequestValidatorV1 : Validator<UpdateUserProfileRequestV1>
{
    public UpdateUserProfileRequestValidatorV1()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.")
            .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("First name contains invalid characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.")
            .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Last name contains invalid characters.");

        RuleFor(x => x.MiddleName)
            .MaximumLength(50).WithMessage("Middle name must not exceed 50 characters.")
            .Matches(@"^[a-zA-Z\s\-'\.]*$").WithMessage("Middle name contains invalid characters.")
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number is not valid.")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}