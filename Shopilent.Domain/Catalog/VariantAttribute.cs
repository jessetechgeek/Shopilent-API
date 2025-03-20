using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog;

public class VariantAttribute : Entity
{
    private VariantAttribute()
    {
        // Required by EF Core
    }

    private VariantAttribute(ProductVariant variant, Attribute attribute, object value)
    {
        if (variant == null)
            throw new ArgumentNullException(nameof(variant));

        if (attribute == null)
            throw new ArgumentNullException(nameof(attribute));

        VariantId = variant.Id;
        AttributeId = attribute.Id;
        Value = new Dictionary<string, object> { { "value", value } };
    }

    // Add static factory method
    public static VariantAttribute Create(ProductVariant variant, Attribute attribute, object value)
    {
        return new VariantAttribute(variant, attribute, value);
    }

    public Guid VariantId { get; private set; }
    public Guid AttributeId { get; private set; }
    public Dictionary<string, object> Value { get; private set; } = new();

    public void UpdateValue(object newValue)
    {
        Value["value"] = newValue;
    }
}