using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Catalog.Categories.GetPaginatedCategories.V1;

public class GetPaginatedCategoriesRequestValidatorV1 : Validator<GetPaginatedCategoriesRequestV1>
{
    public GetPaginatedCategoriesRequestValidatorV1()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");
    }
}