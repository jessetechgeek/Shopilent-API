using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateVariant.V1;

internal sealed class UpdateVariantCommandValidatorV1 : AbstractValidator<UpdateVariantCommandV1>
{
    public UpdateVariantCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Variant ID is required.");

        RuleFor(v => v.Sku)
            .MaximumLength(100).WithMessage("SKU must not exceed 100 characters.")
            .When(v => !string.IsNullOrEmpty(v.Sku));

        RuleFor(v => v.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to zero.")
            .When(v => v.Price.HasValue);

        RuleFor(v => v.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be greater than or equal to zero.")
            .When(v => v.StockQuantity.HasValue);

        // Images to remove validation
        RuleForEach(v => v.ImagesToRemove)
            .NotEmpty().WithMessage("Image key cannot be empty.")
            .When(v => v.ImagesToRemove != null && v.ImagesToRemove.Any());
            
        // Image order validation
        RuleForEach(v => v.ImageOrders)
            .ChildRules(order =>
            {
                order.RuleFor(o => o.ImageKey)
                    .NotEmpty().WithMessage("Image key is required for ordering.");
                order.RuleFor(o => o.DisplayOrder)
                    .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to zero.");
            }).When(v => v.ImageOrders != null && v.ImageOrders.Any());
            
        // Prevent both RemoveExistingImages and ImagesToRemove from being specified
        RuleFor(v => v)
            .Must(v => !(v.RemoveExistingImages == true && v.ImagesToRemove != null && v.ImagesToRemove.Any()))
            .WithMessage("Cannot specify both RemoveExistingImages and ImagesToRemove at the same time.");
            
        // Ensure no duplicate display orders in ImageOrders
        RuleFor(v => v.ImageOrders)
            .Must(orders => orders == null || !orders.GroupBy(o => o.DisplayOrder).Any(g => g.Count() > 1))
            .WithMessage("Duplicate display orders are not allowed in image ordering.");
            
        // Ensure no duplicate image keys in ImageOrders
        RuleFor(v => v.ImageOrders)
            .Must(orders => orders == null || !orders.GroupBy(o => o.ImageKey).Any(g => g.Count() > 1))
            .WithMessage("Duplicate image keys are not allowed in image ordering.");
    }
}