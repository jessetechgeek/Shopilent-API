using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Sales.Events;

public class CartClearedEvent : DomainEvent
{
    public CartClearedEvent(Guid cartId)
    {
        CartId = cartId;
    }

    public Guid CartId { get; }
}