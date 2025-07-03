using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Identity.RefreshToken.V1;

public class RefreshTokenRequestValidatorV1 : Validator<RefreshTokenRequestV1>
{
    public RefreshTokenRequestValidatorV1()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}