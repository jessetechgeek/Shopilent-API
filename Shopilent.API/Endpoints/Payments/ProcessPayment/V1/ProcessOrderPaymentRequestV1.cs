using Shopilent.Domain.Payments.Enums;

namespace Shopilent.API.Endpoints.Payments.ProcessPayment.V1;

public class ProcessOrderPaymentRequestV1
{
    public Guid? PaymentMethodId { get; init; }
    public PaymentMethodType MethodType { get; init; }
    public PaymentProvider Provider { get; init; }
    public string PaymentMethodToken { get; init; }
    public string ExternalReference { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}