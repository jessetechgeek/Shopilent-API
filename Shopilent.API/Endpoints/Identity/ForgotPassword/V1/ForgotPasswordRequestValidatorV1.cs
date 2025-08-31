using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Identity.ForgotPassword.V1;

public class ForgotPasswordRequestValidatorV1 : Validator<ForgotPasswordRequestV1>
{
    public ForgotPasswordRequestValidatorV1()
    {
        RuleFor(x => x.Email)
            .Cascade(FluentValidation.CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
            .EmailAddress().WithMessage("Email is not valid.");
    }
}