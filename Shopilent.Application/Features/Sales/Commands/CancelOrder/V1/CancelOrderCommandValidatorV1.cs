using FluentValidation;

namespace Shopilent.Application.Features.Sales.Commands.CancelOrder.V1;

internal sealed class CancelOrderCommandValidatorV1 : AbstractValidator<CancelOrderCommandV1>
{
    public CancelOrderCommandValidatorV1()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Order ID cannot be empty.");

        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("Current user ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Current user ID cannot be empty.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Cancellation reason must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}