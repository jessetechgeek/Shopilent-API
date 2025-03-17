using Shopilent.Domain.Common;

namespace Shopilent.Domain.Sales.Events;

public class CartCreatedEvent : DomainEvent
{
    public CartCreatedEvent(Guid cartId)
    {
        CartId = cartId;
    }

    public Guid CartId { get; }
}