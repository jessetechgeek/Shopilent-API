using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Sales.Events;

public class OrderCancelledEvent : DomainEvent
{
    public OrderCancelledEvent(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}