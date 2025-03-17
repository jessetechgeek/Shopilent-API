using Shopilent.Domain.Common;

namespace Shopilent.Domain.Payments.Events;

public class PaymentSucceededEvent : DomainEvent
{
    public PaymentSucceededEvent(Guid paymentId, Guid orderId)
    {
        PaymentId = paymentId;
        OrderId = orderId;
    }

    public Guid PaymentId { get; }
    public Guid OrderId { get; }
}