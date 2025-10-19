using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Sales.CreateOrderFromCart.V1;

public class CreateOrderFromCartRequestValidatorV1 : Validator<CreateOrderFromCartRequestV1>
{
    public CreateOrderFromCartRequestValidatorV1()
    {
        RuleFor(x => x.ShippingAddressId)
            .NotEmpty().WithMessage("Shipping address ID is required.");

        RuleFor(x => x.ShippingMethod)
            .MaximumLength(100).WithMessage("Shipping method must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.ShippingMethod));
    }
}