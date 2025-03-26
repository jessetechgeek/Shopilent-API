using Shopilent.Domain.Common;

namespace Shopilent.Domain.Sales.Events;

public class OrderCreatedFromCartEvent : DomainEvent
{
    public OrderCreatedFromCartEvent(Guid orderId, Guid cartId)
    {
        OrderId = orderId;
        CartId = cartId;
    }

    public Guid OrderId { get; }
    public Guid CartId { get; }
}