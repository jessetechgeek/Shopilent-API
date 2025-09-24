using FluentValidation;
using Shopilent.Application.Common.Validators;

namespace Shopilent.Application.Features.Identity.Queries.GetUsersDatatable.V1;

internal sealed class GetUsersDatatableQueryValidatorV1 : AbstractValidator<GetUsersDatatableQueryV1>
{
    public GetUsersDatatableQueryValidatorV1()
    {
        RuleFor(v => v.Request)
            .NotNull().WithMessage("Request is required.")
            .SetValidator(new DataTableRequestValidator());
    }
}