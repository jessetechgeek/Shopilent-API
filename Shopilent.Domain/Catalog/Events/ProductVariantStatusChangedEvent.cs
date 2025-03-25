using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Events;

public class ProductVariantStatusChangedEvent : DomainEvent
{
    public ProductVariantStatusChangedEvent(Guid productId, Guid variantId, bool isActive)
    {
        ProductId = productId;
        VariantId = variantId;
        IsActive = isActive;
    }

    public Guid ProductId { get; }
    public Guid VariantId { get; }
    public bool IsActive { get; }
}