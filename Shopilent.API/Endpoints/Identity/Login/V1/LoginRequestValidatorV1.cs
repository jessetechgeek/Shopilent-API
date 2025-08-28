using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Identity.Login.V1;

public class LoginRequestValidatorV1 : Validator<LoginRequestV1>
{
    public LoginRequestValidatorV1()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(255).EmailAddress().WithMessage("Email is not valid.");

        RuleFor(x => x.Password)
            .MaximumLength(255).NotEmpty().WithMessage("Password is required.");
    }
}
