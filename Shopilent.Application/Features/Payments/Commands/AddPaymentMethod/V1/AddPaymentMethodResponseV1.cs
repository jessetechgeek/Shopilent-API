namespace Shopilent.Application.Features.Payments.Commands.AddPaymentMethod.V1;

public sealed class AddPaymentMethodResponseV1
{
    public Guid Id { get; init; }
    public string Type { get; init; }
    public string Provider { get; init; }
    public string DisplayName { get; init; }
    public string CardBrand { get; init; }
    public string LastFourDigits { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public Dictionary<string, object> Metadata { get; init; }
    public DateTime CreatedAt { get; init; }
}