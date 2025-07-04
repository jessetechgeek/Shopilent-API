using FluentValidation;

namespace Shopilent.Application.Features.Payments.Commands.ProcessOrderPayment.V1;

internal sealed class ProcessOrderPaymentCommandValidatorV1 : AbstractValidator<ProcessOrderPaymentCommandV1>
{
    public ProcessOrderPaymentCommandValidatorV1()
    {
        RuleFor(v => v.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(v => v.MethodType)
            .IsInEnum().WithMessage("Valid payment method type is required.");

        RuleFor(v => v.Provider)
            .IsInEnum().WithMessage("Valid payment provider is required.");

        RuleFor(v => v.PaymentMethodToken)
            .NotEmpty().WithMessage("Payment method token is required.")
            .MaximumLength(500).WithMessage("Payment method token cannot exceed 500 characters.");

        RuleFor(v => v.ExternalReference)
            .MaximumLength(255).WithMessage("External reference cannot exceed 255 characters.")
            .When(v => !string.IsNullOrEmpty(v.ExternalReference));
    }
}