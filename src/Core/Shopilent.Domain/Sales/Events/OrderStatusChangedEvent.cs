using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;
using Shopilent.Domain.Sales.Enums;

namespace Shopilent.Domain.Sales.Events;

public class OrderStatusChangedEvent : DomainEvent
{
    public OrderStatusChangedEvent(Guid orderId, OrderStatus oldStatus, OrderStatus newStatus)
    {
        OrderId = orderId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }

    public Guid OrderId { get; }
    public OrderStatus OldStatus { get; }
    public OrderStatus NewStatus { get; }
}