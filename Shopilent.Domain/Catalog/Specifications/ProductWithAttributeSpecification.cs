using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Specifications;

public class ProductWithAttributeSpecification : Specification<Product>
{
    private readonly Guid _attributeId;
    private readonly object _value;

    public ProductWithAttributeSpecification(Guid attributeId)
    {
        _attributeId = attributeId;
        _value = null;
    }

    public ProductWithAttributeSpecification(Guid attributeId, object value)
    {
        _attributeId = attributeId;
        _value = value;
    }

    public override bool IsSatisfiedBy(Product product)
    {
        var attribute = product.Attributes.FirstOrDefault(a => a.AttributeId == _attributeId);
        if (attribute == null)
            return false;

        if (_value == null)
            return true;

        // Check if the product has the attribute with the specified value
        if (attribute.Values.TryGetValue("value", out var attributeValue))
        {
            return attributeValue?.Equals(_value) == true;
        }

        return false;
    }
}