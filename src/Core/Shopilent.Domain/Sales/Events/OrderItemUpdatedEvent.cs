using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Sales.Events;

public class OrderItemUpdatedEvent : DomainEvent
{
    public OrderItemUpdatedEvent(Guid orderId, Guid orderItemId)
    {
        OrderId = orderId;
        OrderItemId = orderItemId;
    }

    public Guid OrderId { get; }
    public Guid OrderItemId { get; }
}