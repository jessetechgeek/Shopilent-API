using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Users.UpdateUser.V1;

public class UpdateUserRequestValidatorV1 : Validator<UpdateUserRequestV1>
{
    public UpdateUserRequestValidatorV1()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.MiddleName)
            .MaximumLength(50).WithMessage("Middle name cannot exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number format is invalid.")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}