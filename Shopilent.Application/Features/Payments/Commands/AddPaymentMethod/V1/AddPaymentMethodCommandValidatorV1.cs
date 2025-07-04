using FluentValidation;
using Shopilent.Domain.Payments.Enums;

namespace Shopilent.Application.Features.Payments.Commands.AddPaymentMethod.V1;

internal sealed class AddPaymentMethodCommandValidatorV1 : AbstractValidator<AddPaymentMethodCommandV1>
{
    public AddPaymentMethodCommandValidatorV1()
    {
        RuleFor(v => v.Type)
            .NotEmpty().WithMessage("Payment method type is required.")
            .Must(BeValidPaymentMethodType).WithMessage("Invalid payment method type.");

        RuleFor(v => v.Provider)
            .NotEmpty().WithMessage("Payment provider is required.")
            .Must(BeValidPaymentProvider).WithMessage("Invalid payment provider.");

        RuleFor(v => v.Token)
            .NotEmpty().WithMessage("Payment method token is required.")
            .MaximumLength(255).WithMessage("Token cannot exceed 255 characters.");

        RuleFor(v => v.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(255).WithMessage("Display name cannot exceed 255 characters.");

        // Credit card specific validations
        When(v => v.Type == "CreditCard", () =>
        {
            RuleFor(v => v.CardBrand)
                .NotEmpty().WithMessage("Card brand is required for credit cards.")
                .MaximumLength(50).WithMessage("Card brand cannot exceed 50 characters.");

            RuleFor(v => v.LastFourDigits)
                .NotEmpty().WithMessage("Last four digits are required for credit cards.")
                .Matches(@"^\d{4}$").WithMessage("Last four digits must be exactly 4 numeric characters.");

            RuleFor(v => v.ExpiryDate)
                .NotNull().WithMessage("Expiry date is required for credit cards.")
                .GreaterThan(DateTime.UtcNow).WithMessage("Card has expired.");
        });

        // PayPal specific validations
        When(v => v.Type == "PayPal", () =>
        {
            RuleFor(v => v.Email)
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