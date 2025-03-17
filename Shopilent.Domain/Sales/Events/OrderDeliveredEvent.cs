using Shopilent.Domain.Common;

namespace Shopilent.Domain.Sales.Events;

public class OrderDeliveredEvent : DomainEvent
{
    public OrderDeliveredEvent(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}