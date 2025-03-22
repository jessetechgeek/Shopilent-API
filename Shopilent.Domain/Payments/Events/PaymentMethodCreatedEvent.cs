using Shopilent.Domain.Common;

namespace Shopilent.Domain.Payments.Events;

public class PaymentMethodCreatedEvent : DomainEvent
{
    public PaymentMethodCreatedEvent(Guid paymentMethodId, Guid userId)
    {
        PaymentMethodId = paymentMethodId;
        UserId = userId;
    }

    public Guid PaymentMethodId { get; }
    public Guid UserId { get; }
}