namespace Shopilent.API.Endpoints.Payments.AddPaymentMethod.V1;

public class AddPaymentMethodRequestV1
{
    public string Type { get; init; } // "CreditCard", "PayPal"
    public string Provider { get; init; } // "Stripe", "PayPal", "Braintree"
    public string Token { get; init; }
    public string DisplayName { get; init; }
    public bool IsDefault { get; init; } = false;
    
    // For Credit Cards
    public string CardBrand { get; init; }
    public string LastFourDigits { get; init; }
    public DateTime? ExpiryDate { get; init; }
    
    // For PayPal
    public string Email { get; init; }
    
    // Additional metadata
    public Dictionary<string, object> Metadata { get; init; } = new();
}