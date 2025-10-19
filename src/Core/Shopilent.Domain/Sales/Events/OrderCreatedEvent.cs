using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Sales.Events;

public class OrderCreatedEvent : DomainEvent
{
    public OrderCreatedEvent(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}