using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Sales.Events;

public class CartItemUpdatedEvent : DomainEvent
{
    public CartItemUpdatedEvent(Guid cartId, Guid itemId)
    {
        CartId = cartId;
        ItemId = itemId;
    }

    public Guid CartId { get; }
    public Guid ItemId { get; }
}