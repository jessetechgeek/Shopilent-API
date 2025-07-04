using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Payments.Events;

public class PaymentMethodDeletedEvent : DomainEvent
{
    public PaymentMethodDeletedEvent(Guid paymentMethodId, Guid userId)
    {
        PaymentMethodId = paymentMethodId;
        UserId = userId;
    }

    public Guid PaymentMethodId { get; }
    public Guid UserId { get; }
}