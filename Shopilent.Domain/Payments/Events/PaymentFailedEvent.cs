using Shopilent.Domain.Common;

namespace Shopilent.Domain.Payments.Events;

public class PaymentFailedEvent : DomainEvent
{
    public PaymentFailedEvent(Guid paymentId, Guid orderId, string errorMessage)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        ErrorMessage = errorMessage;
    }

    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public string ErrorMessage { get; }
}