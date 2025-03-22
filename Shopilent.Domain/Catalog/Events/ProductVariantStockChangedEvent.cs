using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Events;

public class ProductVariantStockChangedEvent : DomainEvent
{
    public ProductVariantStockChangedEvent(Guid productId, Guid variantId, int oldQuantity, int newQuantity)
    {
        ProductId = productId;
        VariantId = variantId;
        OldQuantity = oldQuantity;
        NewQuantity = newQuantity;
    }

    public Guid ProductId { get; }
    public Guid VariantId { get; }
    public int OldQuantity { get; }
    public int NewQuantity { get; }
}