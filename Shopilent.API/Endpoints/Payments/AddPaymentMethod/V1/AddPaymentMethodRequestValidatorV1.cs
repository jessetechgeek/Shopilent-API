using FastEndpoints;
using FluentValidation;
using Shopilent.Domain.Payments.Enums;

namespace Shopilent.API.Endpoints.Payments.AddPaymentMethod.V1;

public class AddPaymentMethodRequestValidatorV1 : Validator<AddPaymentMethodRequestV1>
{
    public AddPaymentMethodRequestValidatorV1()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Payment method type is required.")
            .Must(BeValidPaymentMethodType).WithMessage("Invalid payment method type. Valid types: CreditCard, PayPal.");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Payment provider is required.")
            .Must(BeValidPaymentProvider).WithMessage("Invalid payment provider. Valid providers: Stripe, PayPal, Braintree.");


        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(255).WithMessage("Display name cannot exceed 255 characters.");

        // Credit card specific validations
        When(x => x.Type == "CreditCard", () =>
        {
            RuleFor(x => x.CardBrand)
                .NotEmpty().WithMessage("Card brand is required for credit cards.")
                .MaximumLength(50).WithMessage("Card brand cannot exceed 50 characters.");

            RuleFor(x => x.LastFourDigits)
                .NotEmpty().WithMessage("Last four digits are required for credit cards.")
                .Matches(@"^\d{4}$").WithMessage("Last four digits must be exactly 4 numeric characters.");

            RuleFor(x => x.ExpiryDate)
                .NotNull().WithMessage("Expiry date is required for credit cards.")
                .GreaterThan(DateTime.UtcNow).WithMessage("Card has expired.");
        });

        // PayPal specific validations
        When(x => x.Type == "PayPal", () =>
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required for PayPal accounts.")
                .EmailAddress().WithMessage("Invalid email format.");
        });
    }

    private static bool BeValidPaymentMethodType(string type)
    {
        return Enum.TryParse<PaymentMethodType>(type, true, out _);
    }

    private static bool BeValidPaymentProvider(string provider)
    {
        return Enum.TryParse<PaymentProvider>(provider, true, out _);
    }
}