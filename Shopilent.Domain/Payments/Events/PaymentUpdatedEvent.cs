using Shopilent.Domain.Common;

namespace Shopilent.Domain.Payments.Events;

public class PaymentUpdatedEvent : DomainEvent
{
    public PaymentUpdatedEvent(Guid paymentId)
    {
        PaymentId = paymentId;
    }

    public Guid PaymentId { get; }
}