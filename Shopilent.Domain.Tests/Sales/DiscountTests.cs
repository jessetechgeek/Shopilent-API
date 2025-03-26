using Shopilent.Domain.Sales.Enums;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Domain.Tests.Sales.ValueObjects;

public class DiscountTests
{
    [Fact]
    public void CreatePercentage_WithValidPercentage_ShouldCreateDiscount()
    {
        // Arrange
        var percentage = 15.5m;
        var code = "SUMMER15";

        // Act
        var result = Discount.CreatePercentage(percentage, code);

        // Assert
        Assert.True(result.IsSuccess);
        var discount = result.Value;
        Assert.Equal(percentage, discount.Value);
        Assert.Equal(DiscountType.Percentage, discount.Type);
        Assert.Equal(code, discount.Code);
    }

    [Fact]
    public void CreatePercentage_WithoutCode_ShouldCreateDiscount()
    {
        // Arrange
        var percentage = 20m;

        // Act
        var result = Discount.CreatePercentage(percentage);

        // Assert
        Assert.True(result.IsSuccess);
        var discount = result.Value;
        Assert.Equal(percentage, discount.Value);
        Assert.Equal(DiscountType.Percentage, discount.Type);
        Assert.Null(discount.Code);
    }

    [Fact]
    public void CreatePercentage_WithNegativePercentage_ShouldReturnFailure()
    {
        // Arrange
        var percentage = -10m;

        // Act
        var result = Discount.CreatePercentage(percentage);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Order.NegativeDiscount", result.Error.Code);
    }

    [Fact]
    public void CreatePercentage_WithOverHundredPercentage_ShouldReturnFailure()
    {
        // Arrange
        var percentage = 110m;

        // Act
        var result = Discount.CreatePercentage(percentage);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Order.InvalidDiscountPercentage", result.Error.Code);
    }

    [Fact]
    public void CreateFixedAmount_WithValidAmount_ShouldCreateDiscount()
    {
        // Arrange
        var amount = 25m;
        var code = "25OFF";

        // Act
        var result = Discount.CreateFixedAmount(amount, code);

        // Assert
        Assert.True(result.IsSuccess);
        var discount = result.Value;
        Assert.Equal(amount, discount.Value);
        Assert.Equal(DiscountType.FixedAmount, discount.Type);
        Assert.Equal(code, discount.Code);
    }

    [Fact]
    public void CreateFixedAmount_WithoutCode_ShouldCreateDiscount()
    {
        // Arrange
        var amount = 10m;

        // Act
        var result = Discount.CreateFixedAmount(amount);

        // Assert
        Assert.True(result.IsSuccess);
        var discount = result.Value;
        Assert.Equal(amount, discount.Value);
        Assert.Equal(DiscountType.FixedAmount, discount.Type);
        Assert.Null(discount.Code);
    }

    [Fact]
    public void CreateFixedAmount_WithNegativeAmount_ShouldReturnFailure()
    {
        // Arrange
        var amount = -5m;

        // Act
        var result = Discount.CreateFixedAmount(amount);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Order.NegativeDiscount", result.Error.Code);
    }

    [Fact]
    public void CalculateDiscount_WithPercentageDiscount_ShouldCalculateCorrectValue()
    {
        // Arrange
        var baseAmountResult = Money.FromDollars(100);
        Assert.True(baseAmountResult.IsSuccess);
        var baseAmount = baseAmountResult.Value;

        var discountResult = Discount.CreatePercentage(15);
        Assert.True(discountResult.IsSuccess);
        var discount = discountResult.Value;

        // Act
        var calculatedResult = discount.CalculateDiscount(baseAmount);

        // Assert
        Assert.True(calculatedResult.IsSuccess);
        var calculatedDiscount = calculatedResult.Value;
        Assert.Equal(15m, calculatedDiscount.Amount);
        Assert.Equal(baseAmount.Currency, calculatedDiscount.Currency);
    }

    [Fact]
    public void CalculateDiscount_WithFixedAmountDiscount_ShouldCalculateCorrectValue()
    {
        // Arrange
        var baseAmountResult = Money.FromDollars(100);
        Assert.True(baseAmountResult.IsSuccess);
        var baseAmount = baseAmountResult.Value;

        var discountResult = Discount.CreateFixedAmount(25);
        Assert.True(discountResult.IsSuccess);
        var discount = discountResult.Value;

        // Act
        var calculatedResult = discount.CalculateDiscount(baseAmount);

        // Assert
        Assert.True(calculatedResult.IsSuccess);
        var calculatedDiscount = calculatedResult.Value;
        Assert.Equal(25m, calculatedDiscount.Amount);
        Assert.Equal(baseAmount.Currency, calculatedDiscount.Currency);
    }

    [Fact]
    public void CalculateDiscount_WithFixedAmountGreaterThanBaseAmount_ShouldCapAtBaseAmount()
    {
        // Arrange
        var baseAmountResult = Money.FromDollars(50);
        Assert.True(baseAmountResult.IsSuccess);
        var baseAmount = baseAmountResult.Value;

        var discountResult = Discount.CreateFixedAmount(75);
        Assert.True(discountResult.IsSuccess);
        var discount = discountResult.Value;

        // Act
        var calculatedResult = discount.CalculateDiscount(baseAmount);

        // Assert
        Assert.True(calculatedResult.IsSuccess);
        var calculatedDiscount = calculatedResult.Value;
        Assert.Equal(50m, calculatedDiscount.Amount); // Capped at base amount
        Assert.Equal(baseAmount.Currency, calculatedDiscount.Currency);
    }

    [Fact]
    public void CalculateDiscount_WithNullBaseAmount_ShouldReturnFailure()
    {
        // Arrange
        Money baseAmount = null;

        var discountResult = Discount.CreatePercentage(15);
        Assert.True(discountResult.IsSuccess);
        var discount = discountResult.Value;

        // Act
        var calculatedResult = discount.CalculateDiscount(baseAmount);

        // Assert
        Assert.True(calculatedResult.IsFailure);
        Assert.Equal("Order.InvalidAmount", calculatedResult.Error.Code);
    }

    [Fact]
    public void ApplyDiscount_WithPercentageDiscount_ShouldReturnDiscountedAmount()
    {
        // Arrange
        var baseAmountResult = Money.FromDollars(100);
        Assert.True(baseAmountResult.IsSuccess);
        var baseAmount = baseAmountResult.Value;

        var discountResult = Discount.CreatePercentage(15);
        Assert.True(discountResult.IsSuccess);
        var discount = discountResult.Value;

        // Act
        var discountedResult = discount.ApplyDiscount(baseAmount);

        // Assert
        Assert.True(discountedResult.IsSuccess);
        var discountedAmount = discountedResult.Value;
        Assert.Equal(85m, discountedAmount.Amount); // $100 - 15%
        Assert.Equal(baseAmount.Currency, discountedAmount.Currency);
    }

    [Fact]
    public void ApplyDiscount_WithFixedAmountDiscount_ShouldReturnDiscountedAmount()
    {
        // Arrange
        var baseAmountResult = Money.FromDollars(100);
        Assert.True(baseAmountResult.IsSuccess);
        var baseAmount = baseAmountResult.Value;

        var discountResult = Discount.CreateFixedAmount(25);
        Assert.True(discountResult.IsSuccess);
        var discount = discountResult.Value;

        // Act
        var discountedResult = discount.ApplyDiscount(baseAmount);

        // Assert
        Assert.True(discountedResult.IsSuccess);
        var discountedAmount = discountedResult.Value;
        Assert.Equal(75m, discountedAmount.Amount); // $100 - $25
        Assert.Equal(baseAmount.Currency, discountedAmount.Currency);
    }

    [Fact]
    public void ApplyDiscount_WithFixedAmountGreaterThanBaseAmount_ShouldReturnZero()
    {
        // Arrange
        var baseAmountResult = Money.FromDollars(50);
        Assert.True(baseAmountResult.IsSuccess);
        var baseAmount = baseAmountResult.Value;

        var discountResult = Discount.CreateFixedAmount(75);
        Assert.True(discountResult.IsSuccess);
        var discount = discountResult.Value;

        // Act
        var discountedResult = discount.ApplyDiscount(baseAmount);

        // Assert
        Assert.True(discountedResult.IsSuccess);
        var discountedAmount = discountedResult.Value;
        Assert.Equal(0m, discountedAmount.Amount); // $50 - $75 = $0 (not negative)
        Assert.Equal(baseAmount.Currency, discountedAmount.Currency);
    }

    [Fact]
    public void ToString_WithPercentageDiscount_ShouldFormatCorrectly()
    {
        // Arrange
        var percentage = 15.5m;
        var discountResult = Discount.CreatePercentage(percentage);
        Assert.True(discountResult.IsSuccess);
        var discount = discountResult.Value;
        var expected = "15.5%";

        // Act
        var result = discount.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToString_WithFixedAmountDiscount_ShouldFormatCorrectly()
    {
        // Arrange
        var amount = 25m;
        var discountResult = Discount.CreateFixedAmount(amount);
        Assert.True(discountResult.IsSuccess);
        var discount = discountResult.Value;

        // Act
        var result = discount.ToString();

        // The exact format will depend on the current culture's currency format
        // So we just check that it contains the amount
        Assert.Contains("25", result);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var discount1Result = Discount.CreatePercentage(15, "SUMMER15");
        var discount2Result = Discount.CreatePercentage(15, "SUMMER15");

        Assert.True(discount1Result.IsSuccess);
        Assert.True(discount2Result.IsSuccess);

        var discount1 = discount1Result.Value;
        var discount2 = discount2Result.Value;

        // Act & Assert
        Assert.Equal(discount1, discount2);
    }
}