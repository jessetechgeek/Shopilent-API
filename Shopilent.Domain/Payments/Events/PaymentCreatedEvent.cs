using Shopilent.Domain.Common;

namespace Shopilent.Domain.Payments.Events;

public class PaymentCreatedEvent : DomainEvent
{
    public PaymentCreatedEvent(Guid paymentId)
    {
        PaymentId = paymentId;
    }

    public Guid PaymentId { get; }
}