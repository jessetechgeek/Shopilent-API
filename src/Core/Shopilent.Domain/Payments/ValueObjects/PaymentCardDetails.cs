using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Payments.Errors;

namespace Shopilent.Domain.Payments.ValueObjects;

public class PaymentCardDetails : ValueObject
{
    public string Brand { get; private set; }
    public string LastFourDigits { get; private set; }
    public DateTime ExpiryDate { get; private set; }

    protected PaymentCardDetails()
    {
    }

    private PaymentCardDetails(string brand, string lastFourDigits, DateTime expiryDate)
    {
        Brand = brand;
        LastFourDigits = lastFourDigits;
        ExpiryDate = expiryDate;
    }

    public static Result<PaymentCardDetails> Create(string brand, string lastFourDigits, DateTime expiryDate)
    {
        if (string.IsNullOrWhiteSpace(brand))
            return Result.Failure<PaymentCardDetails>(PaymentMethodErrors.InvalidCardDetails);

        if (string.IsNullOrWhiteSpace(lastFourDigits) || lastFourDigits.Length != 4 || !lastFourDigits.All(char.IsDigit))
            return Result.Failure<PaymentCardDetails>(PaymentMethodErrors.InvalidCardDetails);

        if (expiryDate < DateTime.UtcNow.Date)
            return Result.Failure<PaymentCardDetails>(PaymentMethodErrors.ExpiredCard);

        return Result.Success(new PaymentCardDetails(brand, lastFourDigits, expiryDate));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Brand;
        yield return LastFourDigits;
        yield return ExpiryDate;
    }
}