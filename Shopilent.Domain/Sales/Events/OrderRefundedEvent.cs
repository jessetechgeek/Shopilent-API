using Shopilent.Domain.Common;

namespace Shopilent.Domain.Sales.Events;

public class OrderRefundedEvent : DomainEvent
{
    public OrderRefundedEvent(Guid orderId, string reason = null)
    {
        OrderId = orderId;
        Reason = reason;
    }

    public Guid OrderId { get; }
    public string Reason { get; }
}