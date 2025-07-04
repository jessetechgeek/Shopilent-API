using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Sales.ClearCart.V1;

public class ClearCartRequestValidatorV1 : Validator<ClearCartRequestV1>
{
    public ClearCartRequestValidatorV1()
    {
        // CartId is optional - if not provided, we'll use the user's cart
        RuleFor(x => x.CartId)
            .NotEqual(Guid.Empty).WithMessage("Cart ID cannot be empty when provided.")
            .When(x => x.CartId.HasValue);
    }
}