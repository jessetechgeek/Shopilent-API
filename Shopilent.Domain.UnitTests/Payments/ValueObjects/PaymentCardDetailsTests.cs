using Shopilent.Domain.Payments.ValueObjects;

namespace Shopilent.Domain.Tests.Payments.ValueObjects;

public class PaymentCardDetailsTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateCardDetails()
    {
        // Arrange
        var brand = "Visa";
        var lastFourDigits = "4242";
        var expiryDate = DateTime.UtcNow.AddYears(1);

        // Act
        var cardDetailsResult = PaymentCardDetails.Create(brand, lastFourDigits, expiryDate);

        // Assert
        Assert.True(cardDetailsResult.IsSuccess);
        var cardDetails = cardDetailsResult.Value;
        Assert.Equal(brand, cardDetails.Brand);
        Assert.Equal(lastFourDigits, cardDetails.LastFourDigits);
        Assert.Equal(expiryDate, cardDetails.ExpiryDate);
    }

    [Fact]
    public void Create_WithEmptyBrand_ShouldReturnFailure()
    {
        // Arrange
        var brand = string.Empty;
        var lastFourDigits = "4242";
        var expiryDate = DateTime.UtcNow.AddYears(1);

        // Act
        var cardDetailsResult = PaymentCardDetails.Create(brand, lastFourDigits, expiryDate);

        // Assert
        Assert.True(cardDetailsResult.IsFailure);
        Assert.Equal("PaymentMethod.InvalidCardDetails", cardDetailsResult.Error.Code);
    }

    [Fact]
    public void Create_WithInvalidLastFourDigits_ShouldReturnFailure()
    {
        // Arrange
        var brand = "Visa";
        var lastFourDigits = "123"; // Not 4 digits
        var expiryDate = DateTime.UtcNow.AddYears(1);

        // Act
        var cardDetailsResult = PaymentCardDetails.Create(brand, lastFourDigits, expiryDate);

        // Assert
        Assert.True(cardDetailsResult.IsFailure);
        Assert.Equal("PaymentMethod.InvalidCardDetails", cardDetailsResult.Error.Code);
    }

    [Fact]
    public void Create_WithNonDigitLastFour_ShouldReturnFailure()
    {
        // Arrange
        var brand = "Visa";
        var lastFourDigits = "123A"; // Contains a letter
        var expiryDate = DateTime.UtcNow.AddYears(1);

        // Act
        var cardDetailsResult = PaymentCardDetails.Create(brand, lastFourDigits, expiryDate);

        // Assert
        Assert.True(cardDetailsResult.IsFailure);
        Assert.Equal("PaymentMethod.InvalidCardDetails", cardDetailsResult.Error.Code);
    }

    [Fact]
    public void Create_WithPastExpiryDate_ShouldReturnFailure()
    {
        // Arrange
        var brand = "Visa";
        var lastFourDigits = "4242";
        var expiryDate = DateTime.UtcNow.AddYears(-1); // Expired

        // Act
        var cardDetailsResult = PaymentCardDetails.Create(brand, lastFourDigits, expiryDate);

        // Assert
        Assert.True(cardDetailsResult.IsFailure);
        Assert.Equal("PaymentMethod.ExpiredCard", cardDetailsResult.Error.Code);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var expiryDate = new DateTime(2025, 12, 31); // Use fixed date for comparison
        var detailsResult1 = PaymentCardDetails.Create("Visa", "4242", expiryDate);
        var detailsResult2 = PaymentCardDetails.Create("Visa", "4242", expiryDate);
        
        Assert.True(detailsResult1.IsSuccess);
        Assert.True(detailsResult2.IsSuccess);
        
        var details1 = detailsResult1.Value;
        var details2 = detailsResult2.Value;

        // Act & Assert
        Assert.True(details1.Equals(details2));
        Assert.True(details1 == details2);
        Assert.False(details1 != details2);
    }

    [Fact]
    public void Equals_WithDifferentBrand_ShouldReturnFalse()
    {
        // Arrange
        var expiryDate = new DateTime(2025, 12, 31);
        var detailsResult1 = PaymentCardDetails.Create("Visa", "4242", expiryDate);
        var detailsResult2 = PaymentCardDetails.Create("Mastercard", "4242", expiryDate);
        
        Assert.True(detailsResult1.IsSuccess);
        Assert.True(detailsResult2.IsSuccess);
        
        var details1 = detailsResult1.Value;
        var details2 = detailsResult2.Value;

        // Act & Assert
        Assert.False(details1.Equals(details2));
        Assert.False(details1 == details2);
        Assert.True(details1 != details2);
    }

    [Fact]
    public void Equals_WithDifferentLastFour_ShouldReturnFalse()
    {
        // Arrange
        var expiryDate = new DateTime(2025, 12, 31);
        var detailsResult1 = PaymentCardDetails.Create("Visa", "4242", expiryDate);
        var detailsResult2 = PaymentCardDetails.Create("Visa", "5555", expiryDate);
        
        Assert.True(detailsResult1.IsSuccess);
        Assert.True(detailsResult2.IsSuccess);
        
        var details1 = detailsResult1.Value;
        var details2 = detailsResult2.Value;

        // Act & Assert
        Assert.False(details1.Equals(details2));
        Assert.False(details1 == details2);
        Assert.True(details1 != details2);
    }

    [Fact]
    public void Equals_WithDifferentExpiryDate_ShouldReturnFalse()
    {
        // Arrange
        var detailsResult1 = PaymentCardDetails.Create("Visa", "4242", new DateTime(2025, 12, 31));
        var detailsResult2 = PaymentCardDetails.Create("Visa", "4242", new DateTime(2026, 12, 31));
        
        Assert.True(detailsResult1.IsSuccess);
        Assert.True(detailsResult2.IsSuccess);
        
        var details1 = detailsResult1.Value;
        var details2 = detailsResult2.Value;

        // Act & Assert
        Assert.False(details1.Equals(details2));
        Assert.False(details1 == details2);
        Assert.True(details1 != details2);
    }
}