using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Sales.AddItemToCart.V1;

public class AddItemToCartRequestValidatorV1 : Validator<AddItemToCartRequestV1>
{
    public AddItemToCartRequestValidatorV1()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100 items.");
    }
}