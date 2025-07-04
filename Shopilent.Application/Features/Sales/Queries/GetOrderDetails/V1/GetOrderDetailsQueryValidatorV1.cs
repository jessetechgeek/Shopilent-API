using FluentValidation;

namespace Shopilent.Application.Features.Sales.Queries.GetOrderDetails.V1;

internal sealed class GetOrderDetailsQueryValidatorV1 : AbstractValidator<GetOrderDetailsQueryV1>
{
    public GetOrderDetailsQueryValidatorV1()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Order ID cannot be empty.");

        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("Current user ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Current user ID cannot be empty.");
    }
}