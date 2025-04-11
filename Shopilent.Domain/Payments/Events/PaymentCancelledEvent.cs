using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Payments.Events;

public class PaymentCancelledEvent : DomainEvent
{
    public PaymentCancelledEvent(Guid paymentId, Guid orderId)
    {
        PaymentId = paymentId;
        OrderId = orderId;
    }

    public Guid PaymentId { get; }
    public Guid OrderId { get; }
}