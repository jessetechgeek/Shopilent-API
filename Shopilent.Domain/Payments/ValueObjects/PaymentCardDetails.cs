using Shopilent.Domain.Common;

namespace Shopilent.Domain.Payments.ValueObjects;

public class PaymentCardDetails : ValueObject
{
    public string Brand { get; private set; }
    public string LastFourDigits { get; private set; }
    public DateTime ExpiryDate { get; private set; }

    protected PaymentCardDetails()
    {
    }

    public PaymentCardDetails(string brand, string lastFourDigits, DateTime expiryDate)
    {
        if (string.IsNullOrWhiteSpace(brand))
            throw new ArgumentException("Card brand cannot be empty", nameof(brand));

        if (string.IsNullOrWhiteSpace(lastFourDigits) || lastFourDigits.Length != 4 || !lastFourDigits.All(char.IsDigit))
            throw new ArgumentException("Last four digits must be exactly 4 digits", nameof(lastFourDigits));

        if (expiryDate < DateTime.UtcNow.Date)
            throw new ArgumentException("Expiry date cannot be in the past", nameof(expiryDate));

        Brand = brand;
        LastFourDigits = lastFourDigits;
        ExpiryDate = expiryDate;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Brand;
        yield return LastFourDigits;
        yield return ExpiryDate;
    }
}