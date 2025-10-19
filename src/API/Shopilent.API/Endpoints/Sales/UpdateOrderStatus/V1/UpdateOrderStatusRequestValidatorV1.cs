using FluentValidation;
using Shopilent.Domain.Sales.Enums;

namespace Shopilent.API.Endpoints.Sales.UpdateOrderStatus.V1;

public class UpdateOrderStatusRequestValidatorV1 : AbstractValidator<UpdateOrderStatusRequestV1>
{
    public UpdateOrderStatusRequestValidatorV1()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status.")
            .NotEqual(OrderStatus.Pending).WithMessage("Cannot update order to pending status manually.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}