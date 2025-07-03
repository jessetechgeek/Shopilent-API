using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.AddProductVariant.V1;

internal sealed class AddProductVariantCommandValidatorV1 : AbstractValidator<AddProductVariantCommandV1>
{
    public AddProductVariantCommandValidatorV1()
    {
        RuleFor(v => v.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(v => v.Sku)
            .MaximumLength(100).WithMessage("SKU must not exceed 100 characters.");

        RuleFor(v => v.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative.")
            .When(v => v.Price.HasValue);

        RuleFor(v => v.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be non-negative.");
    }
}