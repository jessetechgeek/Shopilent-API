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
    public static Result<VariantAttribute> Create(ProductVariant variant, Attribute attribute, object value)
    {
        if (variant == null)
            return Result.Failure<VariantAttribute>(ProductVariantErrors.NotFound(Guid.Empty));

        if (attribute == null)
            return Result.Failure<VariantAttribute>(AttributeErrors.NotFound(Guid.Empty));
            
        if (!attribute.IsVariant)
            return Result.Failure<VariantAttribute>(ProductVariantErrors.NonVariantAttribute(attribute.Name));
            
        if (value == null)
            return Result.Failure<VariantAttribute>(AttributeErrors.InvalidConfigurationFormat);

        return Result.Success(new VariantAttribute(variant, attribute, value));
    }

    public Guid VariantId { get; private set; }
    public Guid AttributeId { get; private set; }
    public Dictionary<string, object> Value { get; private set; } = new();

    public Result UpdateValue(object newValue)
    {
        if (newValue == null)
            return Result.Failure(AttributeErrors.InvalidConfigurationFormat);
            
        Value["value"] = newValue;
        return Result.Success();
    }
}