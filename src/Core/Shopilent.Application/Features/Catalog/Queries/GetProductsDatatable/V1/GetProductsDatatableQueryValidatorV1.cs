using FluentValidation;
using Shopilent.Application.Common.Validators;

namespace Shopilent.Application.Features.Catalog.Queries.GetProductsDatatable.V1;

internal sealed class GetProductsDatatableQueryValidatorV1 : AbstractValidator<GetProductsDatatableQueryV1>
{
    public GetProductsDatatableQueryValidatorV1()
    {
        RuleFor(v => v.Request)
            .NotNull().WithMessage("Request is required.")
            .SetValidator(new DataTableRequestValidator());
    }
}