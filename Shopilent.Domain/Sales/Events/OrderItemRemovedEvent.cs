using Shopilent.Domain.Common;

namespace Shopilent.Domain.Sales.Events;

public class OrderItemRemovedEvent : DomainEvent
{
    public OrderItemRemovedEvent(Guid orderId, Guid orderItemId)
    {
        OrderId = orderId;
        OrderItemId = orderItemId;
    }

    public Guid OrderId { get; }
    public Guid OrderItemId { get; }
}