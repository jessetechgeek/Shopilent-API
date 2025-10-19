using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Sales.RemoveItemFromCart.V1;

public class RemoveItemFromCartRequestValidatorV1 : Validator<RemoveItemFromCartRequestV1>
{
    public RemoveItemFromCartRequestValidatorV1()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Item ID is required.");
    }
}