using Shopilent.Domain.Common.Errors;

namespace Shopilent.Domain.Catalog.Errors;

public static class ProductVariantErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "ProductVariant.NotFound",
        message: $"Product variant with ID {id} was not found.");

    public static Error DuplicateSku(string sku) => Error.Conflict(
        code: "ProductVariant.DuplicateSku",
        message: $"A product variant with SKU '{sku}' already exists.");

    public static Error NegativePrice => Error.Validation(
        code: "ProductVariant.NegativePrice",
        message: "Product variant price cannot be negative.");

    public static Error NegativeStockQuantity => Error.Validation(
        code: "ProductVariant.NegativeStockQuantity",
        message: "Product variant stock quantity cannot be negative.");

    public static Error InsufficientStock(int requested, int available) => Error.Validation(
        code: "ProductVariant.InsufficientStock",
        message: $"Insufficient stock. Requested: {requested}, Available: {available}.");

    public static Error NonVariantAttribute(string attributeName) => Error.Validation(
        code: "ProductVariant.NonVariantAttribute",
        message: $"The attribute '{attributeName}' is not marked as a variant attribute.");

    public static Error VariantAttributeRequired => Error.Validation(
        code: "ProductVariant.VariantAttributeRequired",
        message: "At least one variant attribute is required to create a product variant.");

    public static Error InactiveVariant => Error.Validation(
        code: "ProductVariant.Inactive",
        message: "Cannot perform operation on inactive product variant.");

    public static Error InvalidMetadataKey => Error.Validation(
        code: "ProductVariant.InvalidMetadataKey",
        message: "Metadata key cannot be empty.");

    public static Error InvalidAttributeValue => Error.Validation(
        code: "ProductVariant.InvalidAttributeValue",
        message: "The attribute value is invalid for the specified attribute type.");

    public static Error DuplicateAttributeCombination => Error.Conflict(
        code: "ProductVariant.DuplicateAttributeCombination",
        message: "A variant with this attribute combination already exists for this product.");
}