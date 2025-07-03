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
            .Must(BeValidSortColumn).WithMessage("Invalid sort column. Valid columns are: Name, BasePrice, CreatedAt, UpdatedAt.");
    }

    private bool BeValidSortColumn(string sortColumn)
    {
        var validColumns = new[] { "Name", "BasePrice", "CreatedAt", "UpdatedAt" };
        return validColumns.Contains(sortColumn, StringComparer.OrdinalIgnoreCase);
    }
}