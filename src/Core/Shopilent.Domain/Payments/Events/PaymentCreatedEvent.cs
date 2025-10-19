using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Payments.Events;

public class PaymentCreatedEvent : DomainEvent
{
    public PaymentCreatedEvent(Guid paymentId)
    {
        PaymentId = paymentId;
    }

    public Guid PaymentId { get; }
}