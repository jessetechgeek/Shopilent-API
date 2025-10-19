using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Commands.CreateProduct.V1;

internal sealed class CreateProductCommandValidatorV1 : AbstractValidator<CreateProductCommandV1>
{
    public CreateProductCommandValidatorV1()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(255).WithMessage("Product name must not exceed 255 characters.");

        RuleFor(v => v.Slug)
            .NotEmpty().WithMessage("Product slug is required.")
            .MaximumLength(255).WithMessage("Product slug must not exceed 255 characters.")
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Product slug must contain only lowercase letters, numbers, and hyphens.");

        RuleFor(v => v.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Base price cannot be negative.");

        RuleFor(v => v.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency code must be 3 characters.");

        RuleFor(v => v.Sku)
            .MaximumLength(100).WithMessage("SKU must not exceed 100 characters.")
            .When(v => !string.IsNullOrEmpty(v.Sku));

        RuleFor(v => v.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(v => !string.IsNullOrEmpty(v.Description));

        RuleForEach(v => v.Attributes)
            .ChildRules(attribute =>
            {
                attribute.RuleFor(a => a.AttributeId)
                    .NotEmpty().WithMessage("Attribute ID is required.");
            });
    }
}