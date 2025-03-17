using Shopilent.Domain.Common;

namespace Shopilent.Domain.Payments.Events;

public class PaymentRefundedEvent : DomainEvent
{
    public PaymentRefundedEvent(Guid paymentId, Guid orderId)
    {
        PaymentId = paymentId;
        OrderId = orderId;
    }

    public Guid PaymentId { get; }
    public Guid OrderId { get; }
}