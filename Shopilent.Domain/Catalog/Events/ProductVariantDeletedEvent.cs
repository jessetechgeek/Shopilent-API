using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Catalog.Events;

public class ProductVariantDeletedEvent : DomainEvent
{
    public ProductVariantDeletedEvent(Guid productId, Guid variantId)
    {
        ProductId = productId;
        VariantId = variantId;
    }

    public Guid ProductId { get; }
    public Guid VariantId { get; }
}