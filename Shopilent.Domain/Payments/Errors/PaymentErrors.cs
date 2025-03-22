using Shopilent.Domain.Common.Errors;

namespace Shopilent.Domain.Payments.Errors;

public static class PaymentErrors
{
    public static Error NegativeAmount => Error.Validation(
        code: "Payment.NegativeAmount",
        message: "Payment amount cannot be negative.");

    public static Error TokenRequired => Error.Validation(
        code: "Payment.TokenRequired",
        message: "Payment token cannot be empty.");
        
    public static Error InvalidPaymentStatus(string operation) => Error.Validation(
        code: "Payment.InvalidStatus",
        message: $"Cannot perform {operation} operation with current payment status.");

    public static Error PaymentMethodNotFound(Guid id) => Error.NotFound(
        code: "Payment.PaymentMethodNotFound",
        message: $"Payment method with ID {id} was not found.");

    public static Error PaymentNotFound(Guid id) => Error.NotFound(
        code: "Payment.NotFound",
        message: $"Payment with ID {id} was not found.");

    public static Error ProcessingFailed(string errorMessage) => Error.Failure(
        code: "Payment.ProcessingFailed",
        message: $"Payment processing failed: {errorMessage}");
        
    public static Error InvalidMetadataKey => Error.Validation(
        code: "Payment.InvalidMetadataKey",
        message: "Metadata key cannot be empty.");
        
    public static Error InvalidProvider => Error.Validation(
        code: "Payment.InvalidProvider",
        message: "The specified payment provider is not supported.");
        
    public static Error InvalidMethodType => Error.Validation(
        code: "Payment.InvalidMethodType",
        message: "The specified payment method type is not supported.");
        
    public static Error CurrencyMismatch => Error.Validation(
        code: "Payment.CurrencyMismatch",
        message: "The payment currency does not match the order currency.");
        
    public static Error AmountMismatch => Error.Validation(
        code: "Payment.AmountMismatch",
        message: "The payment amount does not match the order total.");
}