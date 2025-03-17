using Shopilent.Domain.Common;

namespace Shopilent.Domain.Sales.Events;

public class OrderShippedEvent : DomainEvent
{
    public OrderShippedEvent(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}