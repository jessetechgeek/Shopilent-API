using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Payments.Events;

public class PaymentMethodUpdatedEvent : DomainEvent
{
    public PaymentMethodUpdatedEvent(Guid paymentMethodId)
    {
        PaymentMethodId = paymentMethodId;
    }

    public Guid PaymentMethodId { get; }
}