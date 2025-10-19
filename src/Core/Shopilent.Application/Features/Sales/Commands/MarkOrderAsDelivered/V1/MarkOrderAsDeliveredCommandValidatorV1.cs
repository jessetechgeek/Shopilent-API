using FluentValidation;

namespace Shopilent.Application.Features.Sales.Commands.MarkOrderAsDelivered.V1;

internal sealed class MarkOrderAsDeliveredCommandValidatorV1 : AbstractValidator<MarkOrderAsDeliveredCommandV1>
{
    public MarkOrderAsDeliveredCommandValidatorV1()
    {
        RuleFor(v => v.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");
    }
}