using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Catalog.Products.UpdateProduct.V1;

public class UpdateProductRequestValidatorV1 : Validator<UpdateProductRequestV1>
{
    public UpdateProductRequestValidatorV1()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(255).WithMessage("Product name must not exceed 255 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Product slug is required.")
            .MaximumLength(255).WithMessage("Product slug must not exceed 255 characters.")
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Product slug must contain only lowercase letters, numbers, and hyphens.");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Base price must be greater than or equal to zero.");

        RuleFor(x => x.Sku)
            .MaximumLength(100).WithMessage("SKU must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Sku));

        RuleForEach(x => x.Attributes)
            .ChildRules(attribute =>
            {
                attribute.RuleFor(a => a.AttributeId)
                    .NotEmpty().WithMessage("Attribute ID is required.");
            }).When(x => x.Attributes != null && x.Attributes.Any());

        // File validation
        RuleForEach(x => x.File)
            .Must(f => f.Length <= 5 * 1024 * 1024)
            .WithMessage("File size must not exceed 5MB.")
            .When(x => x.File != null && x.File.Any());
            
        // Images to remove validation
        RuleForEach(x => x.ImagesToRemove)
            .NotEmpty().WithMessage("Image key cannot be empty.")
            .When(x => x.ImagesToRemove != null && x.ImagesToRemove.Any());
            
        // Image order validation
        RuleForEach(x => x.ImageOrders)
            .ChildRules(order =>
            {
                order.RuleFor(o => o.ImageKey)
                    .NotEmpty().WithMessage("Image key is required for ordering.");
                order.RuleFor(o => o.DisplayOrder)
                    .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to zero.");
            }).When(x => x.ImageOrders != null && x.ImageOrders.Any());
            
        // Prevent both RemoveExistingImages and ImagesToRemove from being specified
        RuleFor(x => x)
            .Must(x => !(x.RemoveExistingImages == true && x.ImagesToRemove != null && x.ImagesToRemove.Any()))
            .WithMessage("Cannot specify both RemoveExistingImages and ImagesToRemove at the same time.");
            
        // Ensure no duplicate display orders in ImageOrders
        RuleFor(x => x.ImageOrders)
            .Must(orders => orders == null || !orders.GroupBy(o => o.DisplayOrder).Any(g => g.Count() > 1))
            .WithMessage("Duplicate display orders are not allowed in image ordering.");
            
        // Ensure no duplicate image keys in ImageOrders
        RuleFor(x => x.ImageOrders)
            .Must(orders => orders == null || !orders.GroupBy(o => o.ImageKey).Any(g => g.Count() > 1))
            .WithMessage("Duplicate image keys are not allowed in image ordering.");
    }
}