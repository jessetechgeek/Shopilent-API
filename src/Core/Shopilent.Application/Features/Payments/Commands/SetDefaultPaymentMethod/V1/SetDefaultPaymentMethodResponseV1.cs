namespace Shopilent.Application.Features.Payments.Commands.SetDefaultPaymentMethod.V1;

public sealed class SetDefaultPaymentMethodResponseV1
{
    public Guid PaymentMethodId { get; init; }
    public bool IsDefault { get; init; }
    public string DisplayName { get; init; }
    public DateTime UpdatedAt { get; init; }
}