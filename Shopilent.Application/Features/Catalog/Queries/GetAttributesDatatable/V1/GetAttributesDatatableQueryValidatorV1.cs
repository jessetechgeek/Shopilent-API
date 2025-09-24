using FluentValidation;
using Shopilent.Application.Common.Validators;

namespace Shopilent.Application.Features.Catalog.Queries.GetAttributesDatatable.V1;

internal sealed class GetAttributesDatatableQueryValidatorV1 : AbstractValidator<GetAttributesDatatableQueryV1>
{
    public GetAttributesDatatableQueryValidatorV1()
    {
        RuleFor(v => v.Request)
            .NotNull().WithMessage("Request is required.")
            .SetValidator(new DataTableRequestValidator());
    }
}