using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Sales.AssignCartToUser.V1;

public class AssignCartToUserRequestValidatorV1 : Validator<AssignCartToUserRequestV1>
{
    public AssignCartToUserRequestValidatorV1()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Cart ID is required.");
    }
}