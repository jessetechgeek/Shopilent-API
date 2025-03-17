using Shopilent.Domain.Common;
using Shopilent.Domain.Payments.Enums;

namespace Shopilent.Domain.Sales.Events;

public class OrderPaymentStatusChangedEvent : DomainEvent
{
    public OrderPaymentStatusChangedEvent(Guid orderId, PaymentStatus oldStatus, PaymentStatus newStatus)
    {
        OrderId = orderId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }

    public Guid OrderId { get; }
    public PaymentStatus OldStatus { get; }
    public PaymentStatus NewStatus { get; }
}