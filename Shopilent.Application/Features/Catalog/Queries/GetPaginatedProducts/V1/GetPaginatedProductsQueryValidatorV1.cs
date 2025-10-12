using FluentValidation;

namespace Shopilent.Application.Features.Catalog.Queries.GetPaginatedProducts.V1;

internal sealed class GetPaginatedProductsQueryValidatorV1 : AbstractValidator<GetPaginatedProductsQueryV1>
{
    public GetPaginatedProductsQueryValidatorV1()
    {
        RuleFor(v => v.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(v => v.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Page size must be less than or equal to 1000.");

        RuleFor(v => v.PriceMin)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum price must be greater than or equal to 0.")
            .When(v => v.PriceMin.HasValue);

        RuleFor(v => v.PriceMax)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum price must be greater than or equal to 0.")
            .When(v => v.PriceMax.HasValue);

        RuleFor(v => v)
            .Must(v => !v.PriceMin.HasValue || !v.PriceMax.HasValue || v.PriceMin.Value <= v.PriceMax.Value)
            .WithMessage("Minimum price must be less than or equal to maximum price.")
            .When(v => v.PriceMin.HasValue && v.PriceMax.HasValue);

        RuleForEach(v => v.CategorySlugs)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Category slug must contain only lowercase letters, numbers, and hyphens.")
            .When(v => v.CategorySlugs != null && v.CategorySlugs.Length > 0);
    }
}
