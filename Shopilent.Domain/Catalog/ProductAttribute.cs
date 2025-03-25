using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Domain.Catalog;

public class ProductAttribute : Entity
{
    private ProductAttribute()
    {
        // Required by EF Core
    }

    private ProductAttribute(Product product, Attribute attribute, object value)
    {
        ProductId = product.Id;
        AttributeId = attribute.Id;
        Values = new Dictionary<string, object> { { "value", value } };
    }

    // Add static factory method
    internal static ProductAttribute Create(Product product, Attribute attribute, object value)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (attribute == null)
            throw new ArgumentNullException(nameof(attribute));

        if (value == null)
            throw new ArgumentException("Value cannot be null", nameof(value));

        return new ProductAttribute(product, attribute, value);
    }

    // For use by the Product aggregate which should validate inputs
    internal static Result<ProductAttribute> Create(Result<Product> productResult, Attribute attribute, object value)
    {
        if (productResult.IsFailure)
            return Result.Failure<ProductAttribute>(productResult.Error);

        if (attribute == null)
            return Result.Failure<ProductAttribute>(AttributeErrors.NotFound(Guid.Empty));

        if (value == null)
            return Result.Failure<ProductAttribute>(AttributeErrors.InvalidConfigurationFormat);

        return Result.Success(new ProductAttribute(productResult.Value, attribute, value));
    }

    public Guid ProductId { get; private set; }
    public Guid AttributeId { get; private set; }
    public Dictionary<string, object> Values { get; private set; } = new();

    internal Result UpdateValue(object value)
    {
        if (value == null)
            return Result.Failure(AttributeErrors.InvalidConfigurationFormat);

        Values["value"] = value;
        return Result.Success();
    }

    internal Result AddValue(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Result.Failure(AttributeErrors.InvalidConfigurationFormat);

        if (value == null)
            return Result.Failure(AttributeErrors.InvalidConfigurationFormat);

        Values[key] = value;
        return Result.Success();
    }
}