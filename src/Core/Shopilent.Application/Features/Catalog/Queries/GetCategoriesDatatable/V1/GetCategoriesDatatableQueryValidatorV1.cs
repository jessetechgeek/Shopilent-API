using FluentValidation;
using Shopilent.Application.Common.Validators;

namespace Shopilent.Application.Features.Catalog.Queries.GetCategoriesDatatable.V1;

internal sealed class GetCategoriesDatatableQueryValidatorV1 : AbstractValidator<GetCategoriesDatatableQueryV1>
{
    public GetCategoriesDatatableQueryValidatorV1()
    {
        RuleFor(v => v.Request)
            .NotNull().WithMessage("Request is required.")
            .SetValidator(new DataTableRequestValidator());
    }
}