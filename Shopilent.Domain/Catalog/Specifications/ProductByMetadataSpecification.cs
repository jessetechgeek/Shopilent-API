using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Specifications;

namespace Shopilent.Domain.Catalog.Specifications;

public class ProductByMetadataSpecification : Specification<Product>
{
    private readonly string _key;
    private readonly object _value;

    public ProductByMetadataSpecification(string key, object value)
    {
        _key = key;
        _value = value;
    }

    public override bool IsSatisfiedBy(Product product)
    {
        if (product.Metadata.TryGetValue(_key, out var metadataValue))
        {
            return metadataValue?.Equals(_value) == true;
        }

        return false;
    }
}