using Shopilent.Domain.Common;

namespace Shopilent.Domain.Sales.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    protected Money()
    {
    }

    private Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "USD")
    {
        return new Money(amount, currency);
    }

    public static Money FromDollars(decimal dollars)
    {
        return new Money(dollars, "USD");
    }

    public static Money FromEuros(decimal euros)
    {
        return new Money(euros, "EUR");
    }

    public static Money Zero(string currency = "USD") => new Money(0, currency);

    public Money Add(Money money)
    {
        if (Currency != money.Currency)
            throw new InvalidOperationException(
                $"Cannot add money with different currencies: {Currency} and {money.Currency}");

        return new Money(Amount + money.Amount, Currency);
    }

    public Money Subtract(Money money)
    {
        if (Currency != money.Currency)
            throw new InvalidOperationException(
                $"Cannot subtract money with different currencies: {Currency} and {money.Currency}");

        var result = Amount - money.Amount;
        if (result < 0)
            throw new InvalidOperationException("Result cannot be negative");

        return new Money(result, Currency);
    }

    public Money Multiply(decimal multiplier)
    {
        if (multiplier < 0)
            throw new ArgumentException("Multiplier cannot be negative", nameof(multiplier));

        return new Money(Amount * multiplier, Currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}