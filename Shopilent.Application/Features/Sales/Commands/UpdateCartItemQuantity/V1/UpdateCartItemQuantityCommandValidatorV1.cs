using FluentValidation;

namespace Shopilent.Application.Features.Sales.Commands.UpdateCartItemQuantity.V1;

internal sealed class UpdateCartItemQuantityCommandValidatorV1 : AbstractValidator<UpdateCartItemQuantityCommandV1>
{
    public UpdateCartItemQuantityCommandValidatorV1()
    {
        RuleFor(v => v.CartItemId)
            .NotEmpty().WithMessage("Cart item ID is required.");

        RuleFor(v => v.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
    }
}