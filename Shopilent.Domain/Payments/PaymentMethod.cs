using Shopilent.Domain.Common;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Payments.ValueObjects;

namespace Shopilent.Domain.Payments;

public class PaymentMethod : AggregateRoot
{
    private PaymentMethod()
    {
        // Required by EF Core
    }

    private PaymentMethod(
        User user,
        PaymentMethodType type,
        PaymentProvider provider,
        string token,
        string displayName,
        bool isDefault = false)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty", nameof(displayName));

        UserId = user.Id;
        Type = type;
        Provider = provider;
        Token = token;
        DisplayName = displayName;
        ExpiryDate = null;
        IsDefault = isDefault;
        IsActive = true;
        Metadata = new Dictionary<string, object>();
    }

    private PaymentMethod(
        User user,
        PaymentMethodType type,
        PaymentProvider provider,
        string token,
        string displayName,
        PaymentCardDetails cardDetails,
        bool isDefault = false)
        : this(user, type, provider, token, displayName, isDefault)
    {
        if (cardDetails != null)
        {
            CardBrand = cardDetails.Brand;
            LastFourDigits = cardDetails.LastFourDigits;
            ExpiryDate = cardDetails.ExpiryDate;
        }
    }

    public static PaymentMethod CreateCardMethod(
        User user,
        PaymentProvider provider,
        string token,
        PaymentCardDetails cardDetails,
        bool isDefault = false)
    {
        // Add explicit null check for cardDetails
        if (cardDetails == null)
            throw new ArgumentNullException(nameof(cardDetails));

        string displayName = $"{cardDetails.Brand} ending in {cardDetails.LastFourDigits}";
        return new PaymentMethod(user, PaymentMethodType.CreditCard, provider, token, displayName, cardDetails,
            isDefault);
    }

    public static PaymentMethod CreatePayPalMethod(
        User user,
        string token,
        string email,
        bool isDefault = false)
    {
        var method = new PaymentMethod(user, PaymentMethodType.PayPal, PaymentProvider.PayPal, token,
            $"PayPal ({email})", isDefault);
        method.UpdateMetadata("email", email);
        return method;
    }

    public Guid UserId { get; private set; }
    public PaymentMethodType Type { get; private set; }
    public PaymentProvider Provider { get; private set; }
    public string Token { get; private set; }
    public string DisplayName { get; private set; }
    public string CardBrand { get; private set; }
    public string LastFourDigits { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    public void UpdateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty", nameof(displayName));

        DisplayName = displayName;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        Token = token;
    }

    public void UpdateCardDetails(PaymentCardDetails cardDetails)
    {
        if (Type != PaymentMethodType.CreditCard)
            throw new InvalidOperationException("Only credit card payment methods can have card details");

        if (cardDetails == null)
            throw new ArgumentNullException(nameof(cardDetails));

        CardBrand = cardDetails.Brand;
        LastFourDigits = cardDetails.LastFourDigits;
        ExpiryDate = cardDetails.ExpiryDate;
        DisplayName = $"{cardDetails.Brand} ending in {cardDetails.LastFourDigits}";
    }

    public void UpdateMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty", nameof(key));

        Metadata[key] = value;
    }
}