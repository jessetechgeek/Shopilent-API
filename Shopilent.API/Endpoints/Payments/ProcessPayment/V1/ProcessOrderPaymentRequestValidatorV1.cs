using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Payments.ProcessPayment.V1;

public class ProcessOrderPaymentRequestValidatorV1 : Validator<ProcessOrderPaymentRequestV1>
{
    public ProcessOrderPaymentRequestValidatorV1()
    {
        RuleFor(x => x.MethodType)
            .IsInEnum().WithMessage("Valid payment method type is required.");

        RuleFor(x => x.Provider)
            .IsInEnum().WithMessage("Valid payment provider is required.");

        // Either PaymentMethodId or PaymentMethodToken is required
        RuleFor(x => x)
            .Must(x => x.PaymentMethodId.HasValue || !string.IsNullOrEmpty(x.PaymentMethodToken))
            .WithMessage("Either PaymentMethodId or PaymentMethodToken is required.");

        // PaymentMethodId validation when provided
        RuleFor(x => x.PaymentMethodId)
            .NotEmpty().WithMessage("PaymentMethodId cannot be empty when provided.")
            .When(x => x.PaymentMethodId.HasValue);

        // PaymentMethodToken validation when provided
        RuleFor(x => x.PaymentMethodToken)
            .NotEmpty().WithMessage("PaymentMethodToken cannot be empty when provided.")
            .MaximumLength(500).WithMessage("PaymentMethodToken cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.PaymentMethodToken));

        // Cannot have both PaymentMethodId and PaymentMethodToken
        RuleFor(x => x)
            .Must(x => !(x.PaymentMethodId.HasValue && !string.IsNullOrEmpty(x.PaymentMethodToken)))
            .WithMessage("Cannot specify both PaymentMethodId and PaymentMethodToken. Use one or the other.");

        RuleFor(x => x.ExternalReference)
            .MaximumLength(255).WithMessage("External reference cannot exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.ExternalReference));
    }
}