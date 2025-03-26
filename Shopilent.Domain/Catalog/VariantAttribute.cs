using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Domain.Catalog;

public class VariantAttribute : Entity
{
    private VariantAttribute()
    {
        // Required by EF Core
    }

    private VariantAttribute(ProductVariant variant, Attribute attribute, object value)
    {
        VariantId = variant.Id;
        AttributeId = attribute.Id;
        Value = new Dictionary<string, object> { { "value", value } };
    }

    // Add static factory method
    internal static VariantAttribute Create(ProductVariant variant, Attribute attribute, object value)
    {
        if (variant == null)
            throw new ArgumentNullException(nameof(variant));

        if (attribute == null)
            throw new ArgumentNullException(nameof(attribute));

        if (!attribute.IsVariant)
            throw new ArgumentException($"Attribute '{attribute.Name}' is not a variant attribute", nameof(attribute));

        if (value == null)
            throw new ArgumentException("Value cannot be null", nameof(value));

        return new VariantAttribute(variant, attribute, value);
    }

    // For use by the ProductVariant entity which should validate inputs
    internal static Result<VariantAttribute> Create(Result<ProductVariant> variantResult, Attribute attribute,
        object value)
    {
        if (variantResult.IsFailure)
            return Result.Failure<VariantAttribute>(variantResult.Error);

        if (attribute == null)
            return Result.Failure<VariantAttribute>(AttributeErrors.NotFound(Guid.Empty));

        if (!attribute.IsVariant)
            return Result.Failure<VariantAttribute>(ProductVariantErrors.NonVariantAttribute(attribute.Name));

        if (value == null)
            return Result.Failure<VariantAttribute>(AttributeErrors.InvalidConfigurationFormat);

        return Result.Success(new VariantAttribute(variantResult.Value, attribute, value));
    }

    public Guid VariantId { get; private set; }
    public Guid AttributeId { get; private set; }
    public Dictionary<string, object> Value { get; private set; } = new();

    internal Result UpdateValue(object newValue)
    {
        if (newValue == null)
            return Result.Failure(AttributeErrors.InvalidConfigurationFormat);

        Value["value"] = newValue;
        return Result.Success();
    }
}