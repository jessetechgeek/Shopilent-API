using FluentValidation;
using Shopilent.Domain.Sales.Enums;

namespace Shopilent.Application.Features.Sales.Commands.UpdateOrderStatus.V1;

internal sealed class UpdateOrderStatusCommandValidatorV1 : AbstractValidator<UpdateOrderStatusCommandV1>
{
    public UpdateOrderStatusCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(v => v.Status)
            .IsInEnum().WithMessage("Invalid order status.")
            .NotEqual(OrderStatus.Pending).WithMessage("Cannot update order to pending status manually.");

        RuleFor(v => v.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(v => !string.IsNullOrEmpty(v.Reason));
    }
}