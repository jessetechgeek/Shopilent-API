using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog;

public class ProductAttribute : Entity
{
    private ProductAttribute()
    {
        // Required by EF Core
    }

    private ProductAttribute(Product product, Attribute attribute, object value)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (attribute == null)
            throw new ArgumentNullException(nameof(attribute));

        ProductId = product.Id;
        AttributeId = attribute.Id;
        Values = new Dictionary<string, object> { { "value", value } };
    }

    // Add static factory method
    public static ProductAttribute Create(Product product, Attribute attribute, object value)
    {
        return new ProductAttribute(product, attribute, value);
    }

    public Guid ProductId { get; private set; }
    public Guid AttributeId { get; private set; }
    public Dictionary<string, object> Values { get; private set; } = new();

    public void UpdateValue(object value)
    {
        Values["value"] = value;
    }

    public void AddValue(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Value key cannot be empty", nameof(key));

        Values[key] = value;
    }
}