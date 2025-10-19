using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Catalog.Events;

public class ProductVariantAttributeUpdatedEvent : DomainEvent
{
    public ProductVariantAttributeUpdatedEvent(Guid productId, Guid variantId, Guid attributeId)
    {
        ProductId = productId;
        VariantId = variantId;
        AttributeId = attributeId;
    }

    public Guid ProductId { get; }
    public Guid VariantId { get; }
    public Guid AttributeId { get; }
}