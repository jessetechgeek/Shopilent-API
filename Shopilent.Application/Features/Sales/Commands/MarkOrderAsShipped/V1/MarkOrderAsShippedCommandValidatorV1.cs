using FluentValidation;

namespace Shopilent.Application.Features.Sales.Commands.MarkOrderAsShipped.V1;

internal sealed class MarkOrderAsShippedCommandValidatorV1 : AbstractValidator<MarkOrderAsShippedCommandV1>
{
    public MarkOrderAsShippedCommandValidatorV1()
    {
        RuleFor(v => v.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(v => v.TrackingNumber)
            .MaximumLength(100).WithMessage("Tracking number must not exceed 100 characters.")
            .When(v => !string.IsNullOrEmpty(v.TrackingNumber));
    }
}