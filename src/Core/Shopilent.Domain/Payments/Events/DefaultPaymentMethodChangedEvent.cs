using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Payments.Events;

public class DefaultPaymentMethodChangedEvent : DomainEvent
{
    public DefaultPaymentMethodChangedEvent(Guid paymentMethodId, Guid userId)
    {
        PaymentMethodId = paymentMethodId;
        UserId = userId;
    }

    public Guid PaymentMethodId { get; }
    public Guid UserId { get; }
}