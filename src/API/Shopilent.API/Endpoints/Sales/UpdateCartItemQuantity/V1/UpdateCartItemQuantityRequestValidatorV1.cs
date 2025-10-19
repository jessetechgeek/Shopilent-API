using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Sales.UpdateCartItemQuantity.V1;

public class UpdateCartItemQuantityRequestValidatorV1 : Validator<UpdateCartItemQuantityRequestV1>
{
    public UpdateCartItemQuantityRequestValidatorV1()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(999).WithMessage("Quantity cannot exceed 999.");
    }
}