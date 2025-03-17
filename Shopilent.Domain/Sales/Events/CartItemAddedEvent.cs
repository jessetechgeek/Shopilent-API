using Shopilent.Domain.Common;

namespace Shopilent.Domain.Sales.Events;

public class CartItemAddedEvent : DomainEvent
{
    public CartItemAddedEvent(Guid cartId, Guid itemId)
    {
        CartId = cartId;
        ItemId = itemId;
    }

    public Guid CartId { get; }
    public Guid ItemId { get; }
}