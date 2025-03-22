using Shopilent.Domain.Common;

namespace Shopilent.Domain.Payments.Events;

public class PaymentMethodUpdatedEvent : DomainEvent
{
    public PaymentMethodUpdatedEvent(Guid paymentMethodId)
    {
        PaymentMethodId = paymentMethodId;
    }

    public Guid PaymentMethodId { get; }
}