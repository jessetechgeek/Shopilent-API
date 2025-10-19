using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Payments.Events;

public class PaymentMethodRemovedEvent : DomainEvent
{
    public PaymentMethodRemovedEvent(Guid paymentMethodId, Guid userId)
    {
        PaymentMethodId = paymentMethodId;
        UserId = userId;
    }

    public Guid PaymentMethodId { get; }
    public Guid UserId { get; }
}