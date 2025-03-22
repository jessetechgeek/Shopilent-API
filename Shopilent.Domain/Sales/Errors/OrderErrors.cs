using Shopilent.Domain.Common.Errors;

namespace Shopilent.Domain.Sales.Errors;

public static class OrderErrors
{
    public static Error NegativeAmount => Error.Validation(
        code: "Order.NegativeAmount",
        message: "Order amount cannot be negative.");

    public static Error EmptyCart => Error.Validation(
        code: "Order.EmptyCart",
        message: "Cannot create order from empty cart.");
    
    public static Error InvalidOrderStatus(string operation) => Error.Validation(
        code: "Order.InvalidStatus",
        message: $"Cannot perform {operation} operation with current order status.");

    public static Error ShippingAddressRequired => Error.Validation(
        code: "Order.ShippingAddressRequired",
        message: "Shipping address is required.");

    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Order.NotFound",
        message: $"Order with ID {id} was not found.");

    public static Error PaymentRequired => Error.Validation(
        code: "Order.PaymentRequired",
        message: "Payment is required before shipping.");
    
    public static Error InvalidQuantity => Error.Validation(
        code: "Order.InvalidQuantity",
        message: "Order item quantity must be positive.");
    
    public static Error InvalidMetadataKey => Error.Validation(
        code: "Order.InvalidMetadataKey",
        message: "Metadata key cannot be empty.");
    
    public static Error NegativeDiscount => Error.Validation(
        code: "Order.NegativeDiscount",
        message: "Discount amount cannot be negative.");

    public static Error InvalidDiscountPercentage => Error.Validation(
        code: "Order.InvalidDiscountPercentage",
        message: "Discount percentage cannot exceed 100%.");

    public static Error InvalidAmount => Error.Validation(
        code: "Order.InvalidAmount",
        message: "Invalid order amount.");

    public static Error InvalidCurrency => Error.Validation(
        code: "Order.InvalidCurrency",
        message: "Currency code cannot be empty.");

    public static Error CurrencyMismatch => Error.Validation(
        code: "Order.CurrencyMismatch",
        message: "Operations can only be performed on money objects with the same currency.");
}