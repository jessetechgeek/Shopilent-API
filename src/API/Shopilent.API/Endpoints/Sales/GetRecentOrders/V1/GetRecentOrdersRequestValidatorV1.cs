using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Sales.GetRecentOrders.V1;

public class GetRecentOrdersRequestValidatorV1 : Validator<GetRecentOrdersRequestV1>
{
    public GetRecentOrdersRequestValidatorV1()
    {
        RuleFor(x => x.Count)
            .GreaterThan(0).WithMessage("Count must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Count must not exceed 100.");
    }
}