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
    public static Result<ProductAttribute> Create(Product product, Attribute attribute, object value)
    {
        if (product == null)
            return Result.Failure<ProductAttribute>(ProductErrors.NotFound(Guid.Empty));

        if (attribute == null)
            return Result.Failure<ProductAttribute>(AttributeErrors.NotFound(Guid.Empty));
            
        if (value == null)
            return Result.Failure<ProductAttribute>(AttributeErrors.InvalidConfigurationFormat);

        return Result.Success(new ProductAttribute(product, attribute, value));
    }

    public Guid ProductId { get; private set; }
    public Guid AttributeId { get; private set; }
    public Dictionary<string, object> Values { get; private set; } = new();

    public Result UpdateValue(object value)
    {
        if (value == null)
            return Result.Failure(AttributeErrors.InvalidConfigurationFormat);
            
        Values["value"] = value;
        return Result.Success();
    }

    public Result AddValue(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Result.Failure(AttributeErrors.InvalidConfigurationFormat);

        if (value == null)
            return Result.Failure(AttributeErrors.InvalidConfigurationFormat);

        Values[key] = value;
        return Result.Success();
    }
}