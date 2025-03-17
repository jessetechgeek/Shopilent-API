using Shopilent.Domain.Common;

namespace Shopilent.Domain.Sales.Events;

public class CartClearedEvent : DomainEvent
{
    public CartClearedEvent(Guid cartId)
    {
        CartId = cartId;
    }

    public Guid CartId { get; }
}