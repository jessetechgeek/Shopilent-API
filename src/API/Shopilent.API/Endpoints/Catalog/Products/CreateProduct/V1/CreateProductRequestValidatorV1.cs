using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Catalog.Products.CreateProduct.V1;

public class CreateProductRequestValidatorV1 : Validator<CreateProductRequestV1>
{
    public CreateProductRequestValidatorV1()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(255).WithMessage("Product name must not exceed 255 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Product slug is required.")
            .MaximumLength(255).WithMessage("Product slug must not exceed 255 characters.")
            .Matches("^[a-z0-9-]+$").WithMessage("Product slug must contain only lowercase letters, numbers, and hyphens.");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Base price cannot be negative.");
            
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency code must be 3 characters.");
            
        RuleFor(x => x.Sku)
            .MaximumLength(100).WithMessage("SKU must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Sku));
            
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
        
        RuleForEach(x => x.Attributes)
            .ChildRules(attribute => {
                attribute.RuleFor(a => a.AttributeId)
                    .NotEmpty().WithMessage("Attribute ID is required.");
            });
            
        // RuleForEach(x => x.Images)
        //     .ChildRules(image => {
        //         image.RuleFor(i => i.Url)
        //             .NotEmpty().WithMessage("Image URL is required.")
        //             .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
        //             .WithMessage("Image URL must be a valid absolute URL.");
        //             
        //         image.RuleFor(i => i.DisplayOrder)
        //             .GreaterThanOrEqualTo(0).WithMessage("Display order cannot be negative.");
        //     });
    }
}