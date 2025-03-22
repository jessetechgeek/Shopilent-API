using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Events;

public class ProductVariantUpdatedEvent : DomainEvent
{
    public ProductVariantUpdatedEvent(Guid productId, Guid variantId)
    {
        ProductId = productId;
        VariantId = variantId;
    }

    public Guid ProductId { get; }
    public Guid VariantId { get; }
}