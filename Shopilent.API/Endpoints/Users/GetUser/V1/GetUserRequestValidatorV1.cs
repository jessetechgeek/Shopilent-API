using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Users.GetUser.V1;

public class GetUserRequestValidatorV1 : Validator<GetUserRequestV1>
{
    public GetUserRequestValidatorV1()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("User ID is required.")
            .NotEqual(Guid.Empty)
            .WithMessage("User ID cannot be empty.");
    }
}
