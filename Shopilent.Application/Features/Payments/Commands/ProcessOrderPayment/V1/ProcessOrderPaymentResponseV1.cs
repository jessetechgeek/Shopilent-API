using Shopilent.Domain.Payments.Enums;

namespace Shopilent.Application.Features.Payments.Commands.ProcessOrderPayment.V1;

public sealed class ProcessOrderPaymentResponseV1
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; }
    public PaymentStatus Status { get; init; }
    public PaymentMethodType MethodType { get; init; }
    public PaymentProvider Provider { get; init; }
    public string TransactionId { get; init; }
    public string ExternalReference { get; init; }
    public DateTime ProcessedAt { get; init; }
    public string Message { get; init; }
}