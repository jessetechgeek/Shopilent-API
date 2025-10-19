using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Sales.Events;

public class OrderItemCancelledEvent : DomainEvent
{
    public OrderItemCancelledEvent(Guid orderId, Guid orderItemId)
    {
        OrderId = orderId;
        OrderItemId = orderItemId;
    }

    public Guid OrderId { get; }
    public Guid OrderItemId { get; }
}