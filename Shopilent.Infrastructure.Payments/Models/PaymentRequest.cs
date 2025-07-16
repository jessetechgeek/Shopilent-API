using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Infrastructure.Payments.Models;

public class PaymentRequest
{
    public Money Amount { get; init; }
    public PaymentMethodType MethodType { get; init; }
    public string PaymentMethodToken { get; init; }
    public string CustomerId { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}