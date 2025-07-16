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

        // Either PaymentMethodId or PaymentMethodToken is required
        RuleFor(v => v)
            .Must(v => v.PaymentMethodId.HasValue || !string.IsNullOrEmpty(v.PaymentMethodToken))
            .WithMessage("Either PaymentMethodId or PaymentMethodToken is required.");

        // PaymentMethodId validation when provided
        RuleFor(v => v.PaymentMethodId)
            .NotEmpty().WithMessage("PaymentMethodId cannot be empty when provided.")
            .When(v => v.PaymentMethodId.HasValue);

        // PaymentMethodToken validation when provided
        RuleFor(v => v.PaymentMethodToken)
            .NotEmpty().WithMessage("PaymentMethodToken cannot be empty when provided.")
            .MaximumLength(500).WithMessage("PaymentMethodToken cannot exceed 500 characters.")
            .When(v => !string.IsNullOrEmpty(v.PaymentMethodToken));

        // Cannot have both PaymentMethodId and PaymentMethodToken
        RuleFor(v => v)
            .Must(v => !(v.PaymentMethodId.HasValue && !string.IsNullOrEmpty(v.PaymentMethodToken)))
            .WithMessage("Cannot specify both PaymentMethodId and PaymentMethodToken. Use one or the other.");

    }
}