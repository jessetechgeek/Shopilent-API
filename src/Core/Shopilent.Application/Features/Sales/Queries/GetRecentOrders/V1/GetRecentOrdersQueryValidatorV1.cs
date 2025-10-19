using FluentValidation;

namespace Shopilent.Application.Features.Sales.Queries.GetRecentOrders.V1;

internal sealed class GetRecentOrdersQueryValidatorV1 : AbstractValidator<GetRecentOrdersQueryV1>
{
    public GetRecentOrdersQueryValidatorV1()
    {
        RuleFor(x => x.Count)
            .GreaterThan(0).WithMessage("Count must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Count must not exceed 100.");
    }
}