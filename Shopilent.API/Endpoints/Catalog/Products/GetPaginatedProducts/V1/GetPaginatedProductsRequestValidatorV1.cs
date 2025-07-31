using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Catalog.Products.GetPaginatedProducts.V1;

public class GetPaginatedProductsRequestValidatorV1 : Validator<GetPaginatedProductsRequestV1>
{
    public GetPaginatedProductsRequestValidatorV1()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");

        RuleFor(x => x.SortColumn)
            .NotEmpty().WithMessage("Sort column cannot be empty.")
            .Must(BeValidSortColumn).WithMessage("Invalid sort column. Valid columns are: name, price, created, updated.");

        RuleFor(x => x.SearchQuery)
            .MaximumLength(500)
            .WithMessage("Search query cannot exceed 500 characters");

        RuleFor(x => x.PriceMin)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PriceMin.HasValue)
            .WithMessage("Minimum price must be greater than or equal to 0");

        RuleFor(x => x.PriceMax)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PriceMax.HasValue)
            .WithMessage("Maximum price must be greater than or equal to 0");

        RuleFor(x => x)
            .Must(x => !x.PriceMin.HasValue || !x.PriceMax.HasValue || x.PriceMin <= x.PriceMax)
            .WithMessage("Minimum price must be less than or equal to maximum price");
    }

    private bool BeValidSortColumn(string sortColumn)
    {
        var validColumns = new[] { "name", "price", "created", "updated" };
        return validColumns.Contains(sortColumn, StringComparer.OrdinalIgnoreCase);
    }
}