using FluentValidation;
using Shopilent.Application.Common.Validators;

namespace Shopilent.Application.Features.Sales.Queries.GetOrdersDatatable.V1;

internal sealed class GetOrdersDatatableQueryValidatorV1 : AbstractValidator<GetOrdersDatatableQueryV1>
{
    public GetOrdersDatatableQueryValidatorV1()
    {
        RuleFor(v => v.Request)
            .NotNull().WithMessage("Request is required.")
            .SetValidator(new DataTableRequestValidator());
    }
}