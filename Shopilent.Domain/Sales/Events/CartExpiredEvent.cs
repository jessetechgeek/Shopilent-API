using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Sales.Events;

public class CartExpiredEvent : DomainEvent
{
    public CartExpiredEvent(Guid cartId)
    {
        CartId = cartId;
    }

    public Guid CartId { get; }
}