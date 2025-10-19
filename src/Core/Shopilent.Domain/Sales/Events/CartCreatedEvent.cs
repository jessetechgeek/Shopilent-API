using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Sales.Events;

public class CartCreatedEvent : DomainEvent
{
    public CartCreatedEvent(Guid cartId)
    {
        CartId = cartId;
    }

    public Guid CartId { get; }
}