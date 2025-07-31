using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Search.UniversalSearch.V1;

public class UniversalSearchRequestValidatorV1 : Validator<UniversalSearchRequestV1>
{
    public UniversalSearchRequestValidatorV1()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.Query)
            .MaximumLength(500)
            .WithMessage("Search query cannot exceed 500 characters");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .WithMessage("Invalid sort field. Valid options: relevance, name, price, created, updated, stock");

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

    private static bool BeValidSortField(string sortBy)
    {
        var validFields = new[] { "relevance", "name", "price", "created", "updated", "stock" };
        return validFields.Contains(sortBy.ToLowerInvariant());
    }
}