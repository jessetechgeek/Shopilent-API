using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Payments.Enums;

namespace Shopilent.Application.Features.Payments.Commands.ProcessOrderPayment.V1;

public sealed record ProcessOrderPaymentCommandV1 : ICommand<ProcessOrderPaymentResponseV1>
{
    public Guid OrderId { get; init; }
    public Guid? PaymentMethodId { get; init; }
    public PaymentMethodType MethodType { get; init; }
    public PaymentProvider Provider { get; init; }
    public string PaymentMethodToken { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}