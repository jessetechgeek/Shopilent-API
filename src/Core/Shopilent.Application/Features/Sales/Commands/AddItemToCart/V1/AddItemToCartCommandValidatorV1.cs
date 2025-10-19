using FluentValidation;

namespace Shopilent.Application.Features.Sales.Commands.AddItemToCart.V1;

internal sealed class AddItemToCartCommandValidatorV1 : AbstractValidator<AddItemToCartCommandV1>
{
    public AddItemToCartCommandValidatorV1()
    {
        RuleFor(v => v.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(v => v.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100 items.");
    }
}