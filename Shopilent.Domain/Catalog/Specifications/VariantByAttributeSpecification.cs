using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Specifications;

public class VariantByAttributeSpecification : Specification<ProductVariant>
{
    private readonly Guid _attributeId;
    private readonly object _value;

    public VariantByAttributeSpecification(Guid attributeId, object value = null)
    {
        _attributeId = attributeId;
        _value = value;
    }

    public override bool IsSatisfiedBy(ProductVariant variant)
    {
        // Check if variant has the specified attribute
        var attribute = variant.Attributes.FirstOrDefault(a => a.AttributeId == _attributeId);
        if (attribute == null)
            return false;

        // If no specific value is required, just check for existence
        if (_value == null)
            return true;

        // Check if the attribute has the specified value
        if (attribute.Value.TryGetValue("value", out var attributeValue))
        {
            return attributeValue?.Equals(_value) == true;
        }

        return false;
    }
}