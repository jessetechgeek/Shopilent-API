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

        RuleFor(x => x.PaymentMethodToken)
            .NotEmpty().WithMessage("Payment method token is required.")
            .MaximumLength(500).WithMessage("Payment method token cannot exceed 500 characters.");

        RuleFor(x => x.ExternalReference)
            .MaximumLength(255).WithMessage("External reference cannot exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.ExternalReference));
    }
}