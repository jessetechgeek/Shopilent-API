using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Catalog.Products.AddProductVariant.V1;

public class AddProductVariantRequestValidatorV1 : Validator<AddProductVariantRequestV1>
{
    public AddProductVariantRequestValidatorV1()
    {
        RuleFor(x => x.Sku)
            .MaximumLength(100).WithMessage("SKU must not exceed 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative.")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be non-negative.");

        RuleFor(x => x.Attributes)
            .NotEmpty().WithMessage("At least one attribute value is required for a variant.");
    }
}