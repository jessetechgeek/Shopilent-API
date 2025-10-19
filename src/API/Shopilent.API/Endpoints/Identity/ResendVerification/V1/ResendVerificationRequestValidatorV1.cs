using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Identity.ResendVerification.V1;

public class ResendVerificationRequestValidatorV1 : Validator<ResendVerificationRequestV1>
{
    public ResendVerificationRequestValidatorV1()
    {
        RuleFor(x => x.Email)
            .Cascade(FluentValidation.CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");
    }
}
