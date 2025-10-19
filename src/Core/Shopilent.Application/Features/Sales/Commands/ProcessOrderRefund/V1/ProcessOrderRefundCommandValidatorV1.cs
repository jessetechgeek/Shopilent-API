using FluentValidation;

namespace Shopilent.Application.Features.Sales.Commands.ProcessOrderRefund.V1;

internal sealed class ProcessOrderRefundCommandValidatorV1 : AbstractValidator<ProcessOrderRefundCommandV1>
{
    public ProcessOrderRefundCommandValidatorV1()
    {
        RuleFor(v => v.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(v => v.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}