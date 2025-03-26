using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Domain.Tests.Sales.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateMoney()
    {
        // Arrange
        var amount = 100m;
        var currency = "USD";

        // Act
        var result = Money.Create(amount, currency);

        // Assert
        Assert.True(result.IsSuccess);
        var money = result.Value;
        Assert.Equal(amount, money.Amount);
        Assert.Equal(currency, money.Currency);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldReturnFailure()
    {
        // Arrange
        var amount = -10m;
        var currency = "USD";

        // Act
        var result = Money.Create(amount, currency);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Order.NegativeAmount", result.Error.Code);
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldReturnFailure()
    {
        // Arrange
        var amount = 100m;
        var currency = string.Empty;

        // Act
        var result = Money.Create(amount, currency);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Order.InvalidCurrency", result.Error.Code);
    }

    [Fact]
    public void FromDollars_ShouldCreateMoneyWithUSD()
    {
        // Arrange
        var amount = 99.99m;

        // Act
        var result = Money.FromDollars(amount);

        // Assert
        Assert.True(result.IsSuccess);
        var money = result.Value;
        Assert.Equal(amount, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void FromEuros_ShouldCreateMoneyWithEUR()
    {
        // Arrange
        var amount = 99.99m;

        // Act
        var result = Money.FromEuros(amount);

        // Assert
        Assert.True(result.IsSuccess);
        var money = result.Value;
        Assert.Equal(amount, money.Amount);
        Assert.Equal("EUR", money.Currency);
    }

    [Fact]
    public void Zero_ShouldCreateZeroMoney()
    {
        // Act
        var money = Money.Zero();

        // Assert
        Assert.Equal(0m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Zero_WithCurrency_ShouldCreateZeroMoneyWithCurrency()
    {
        // Arrange
        var currency = "EUR";

        // Act
        var money = Money.Zero(currency);

        // Assert
        Assert.Equal(0m, money.Amount);
        Assert.Equal(currency, money.Currency);
    }

    [Fact]
    public void Add_ShouldAddAmounts()
    {
        // Arrange
        var money1Result = Money.FromDollars(100);
        Assert.True(money1Result.IsSuccess);
        var money1 = money1Result.Value;
        
        var money2Result = Money.FromDollars(50);
        Assert.True(money2Result.IsSuccess);
        var money2 = money2Result.Value;
        
        var expected = 150m;

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(expected, result.Amount);
        Assert.Equal(money1.Currency, result.Currency);
    }

    [Fact]
    public void Add_WithDifferentCurrencies_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1Result = Money.FromDollars(100);
        Assert.True(money1Result.IsSuccess);
        var money1 = money1Result.Value;
        
        var money2Result = Money.FromEuros(50);
        Assert.True(money2Result.IsSuccess);
        var money2 = money2Result.Value;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => money1.Add(money2));
    }

    [Fact]
    public void AddSafe_WithSameCurrency_ShouldAddAmounts()
    {
        // Arrange
        var money1Result = Money.FromDollars(100);
        Assert.True(money1Result.IsSuccess);
        var money1 = money1Result.Value;
        
        var money2Result = Money.FromDollars(50);
        Assert.True(money2Result.IsSuccess);
        var money2 = money2Result.Value;
        
        var expected = 150m;

        // Act
        var result = money1.AddSafe(money2);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Amount);
        Assert.Equal(money1.Currency, result.Value.Currency);
    }

    [Fact]
    public void AddSafe_WithDifferentCurrencies_ShouldReturnFailure()
    {
        // Arrange
        var money1Result = Money.FromDollars(100);
        Assert.True(money1Result.IsSuccess);
        var money1 = money1Result.Value;
        
        var money2Result = Money.FromEuros(50);
        Assert.True(money2Result.IsSuccess);
        var money2 = money2Result.Value;

        // Act
        var result = money1.AddSafe(money2);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Order.CurrencyMismatch", result.Error.Code);
    }

    [Fact]
    public void Subtract_ShouldSubtractAmounts()
    {
        // Arrange
        var money1Result = Money.FromDollars(100);
        Assert.True(money1Result.IsSuccess);
        var money1 = money1Result.Value;
        
        var money2Result = Money.FromDollars(30);
        Assert.True(money2Result.IsSuccess);
        var money2 = money2Result.Value;
        
        var expected = 70m;

        // Act
        var result = money1.Subtract(money2);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Amount);
        Assert.Equal(money1.Currency, result.Value.Currency);
    }

    [Fact]
    public void Subtract_WithDifferentCurrencies_ShouldReturnFailure()
    {
        // Arrange
        var money1Result = Money.FromDollars(100);
        Assert.True(money1Result.IsSuccess);
        var money1 = money1Result.Value;
        
        var money2Result = Money.FromEuros(50);
        Assert.True(money2Result.IsSuccess);
        var money2 = money2Result.Value;

        // Act
        var result = money1.Subtract(money2);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Order.CurrencyMismatch", result.Error.Code);
    }

    [Fact]
    public void Subtract_ResultingInNegative_ShouldReturnFailure()
    {
        // Arrange
        var money1Result = Money.FromDollars(30);
        Assert.True(money1Result.IsSuccess);
        var money1 = money1Result.Value;
        
        var money2Result = Money.FromDollars(50);
        Assert.True(money2Result.IsSuccess);
        var money2 = money2Result.Value;

        // Act
        var result = money1.Subtract(money2);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Order.NegativeAmount", result.Error.Code);
    }

    [Fact]
    public void Multiply_ShouldMultiplyAmount()
    {
        // Arrange
        var moneyResult = Money.FromDollars(10);
        Assert.True(moneyResult.IsSuccess);
        var money = moneyResult.Value;
        
        var multiplier = 3.5m;
        var expected = 35m;

        // Act
        var result = money.Multiply(multiplier);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Amount);
        Assert.Equal(money.Currency, result.Value.Currency);
    }

    [Fact]
    public void Multiply_WithNegativeMultiplier_ShouldReturnFailure()
    {
        // Arrange
        var moneyResult = Money.FromDollars(10);
        Assert.True(moneyResult.IsSuccess);
        var money = moneyResult.Value;
        
        var multiplier = -2m;

        // Act
        var result = money.Multiply(multiplier);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Order.NegativeAmount", result.Error.Code);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var money1Result = Money.FromDollars(100);
        Assert.True(money1Result.IsSuccess);
        var money1 = money1Result.Value;
        
        var money2Result = Money.FromDollars(100);
        Assert.True(money2Result.IsSuccess);
        var money2 = money2Result.Value;

        // Act & Assert
        Assert.True(money1.Equals(money2));
        Assert.True(money1 == money2);
        Assert.False(money1 != money2);
    }

    [Fact]
    public void Equals_WithDifferentAmounts_ShouldReturnFalse()
    {
        // Arrange
        var money1Result = Money.FromDollars(100);
        Assert.True(money1Result.IsSuccess);
        var money1 = money1Result.Value;
        
        var money2Result = Money.FromDollars(50);
        Assert.True(money2Result.IsSuccess);
        var money2 = money2Result.Value;

        // Act & Assert
        Assert.False(money1.Equals(money2));
        Assert.False(money1 == money2);
        Assert.True(money1 != money2);
    }

    [Fact]
    public void Equals_WithDifferentCurrencies_ShouldReturnFalse()
    {
        // Arrange
        var money1Result = Money.FromDollars(100);
        Assert.True(money1Result.IsSuccess);
        var money1 = money1Result.Value;
        
        var money2Result = Money.FromEuros(100);
        Assert.True(money2Result.IsSuccess);
        var money2 = money2Result.Value;

        // Act & Assert
        Assert.False(money1.Equals(money2));
        Assert.False(money1 == money2);
        Assert.True(money1 != money2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var moneyResult = Money.FromDollars(123.45m);
        Assert.True(moneyResult.IsSuccess);
        var money = moneyResult.Value;
        
        var expected = "123.45 USD";

        // Act
        var result = money.ToString();

        // Assert
        Assert.Equal(expected, result);
    }
}