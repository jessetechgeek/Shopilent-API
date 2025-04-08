using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Domain.Tests.Identity.ValueObjects;

public class PhoneNumberTests
{
    [Fact]
    public void Create_WithValidPhoneNumber_ShouldCreatePhoneNumber()
    {
        // Arrange
        var phoneNumberStr = "+1-555-123-4567";
        var expected = "+15551234567"; // Only digits and + retained

        // Act
        var result = PhoneNumber.Create(phoneNumberStr);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Fact]
    public void Create_WithEmptyPhoneNumber_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumberStr = string.Empty;

        // Act
        var result = PhoneNumber.Create(phoneNumberStr);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.PhoneRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithWhitespacePhoneNumber_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumberStr = "   ";

        // Act
        var result = PhoneNumber.Create(phoneNumberStr);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.PhoneRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithShortPhoneNumber_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumberStr = "12345"; // Less than 7 digits

        // Act
        var result = PhoneNumber.Create(phoneNumberStr);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.InvalidPhoneFormat", result.Error.Code);
    }

    [Fact]
    public void Create_WithSpecialCharacters_ShouldRemoveThem()
    {
        // Arrange
        var phoneNumberStr = "(555) 123-4567";
        var expected = "5551234567";

        // Act
        var result = PhoneNumber.Create(phoneNumberStr);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Fact]
    public void Create_WithLeadingPlus_ShouldPreserveIt()
    {
        // Arrange
        var phoneNumberStr = "+1 555 123 4567";
        var expected = "+15551234567";

        // Act
        var result = PhoneNumber.Create(phoneNumberStr);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Fact]
    public void Create_WithLetters_ShouldRemoveThemAndReturnFailureIfTooShort()
    {
        // Arrange
        var phoneNumberStr = "555-CALL-NOW";

        // Act
        var result = PhoneNumber.Create(phoneNumberStr);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.InvalidPhoneFormat", result.Error.Code);
    }

    [Fact]
    public void Create_WithValidFormattedNumber_ShouldNormalizeFormat()
    {
        // Arrange
        var phoneNumberVariations = new[]
        {
            "+1 (555) 123-4567",
            "+1.555.123.4567",
            "+1-555-123-4567",
            "1-555-123-4567"
        };
        var expected = "+15551234567"; // With leading plus
        var expectedWithoutPlus = "15551234567"; // Without leading plus

        // Act & Assert
        foreach (var number in phoneNumberVariations)
        {
            var result = PhoneNumber.Create(number);
            Assert.True(result.IsSuccess);
            
            if (number.StartsWith("+"))
            {
                Assert.Equal(expected, result.Value.Value);
            }
            else
            {
                Assert.Equal(expectedWithoutPlus, result.Value.Value);
            }
        }
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var result1 = PhoneNumber.Create("+1-555-123-4567");
        var result2 = PhoneNumber.Create("+1 (555) 123-4567");
        
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        
        var phoneNumber1 = result1.Value;
        var phoneNumber2 = result2.Value;

        // Act & Assert
        Assert.True(phoneNumber1.Equals(phoneNumber2));
        Assert.True(phoneNumber1 == phoneNumber2);
        Assert.False(phoneNumber1 != phoneNumber2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var result1 = PhoneNumber.Create("+1-555-123-4567");
        var result2 = PhoneNumber.Create("+1-555-123-4568");
        
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        
        var phoneNumber1 = result1.Value;
        var phoneNumber2 = result2.Value;

        // Act & Assert
        Assert.False(phoneNumber1.Equals(phoneNumber2));
        Assert.False(phoneNumber1 == phoneNumber2);
        Assert.True(phoneNumber1 != phoneNumber2);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var phoneNumberStr = "+1-555-123-4567";
        var expected = "+15551234567";
        var result = PhoneNumber.Create(phoneNumberStr);
        Assert.True(result.IsSuccess);
        var phoneNumber = result.Value;

        // Act
        var stringResult = phoneNumber.ToString();

        // Assert
        Assert.Equal(expected, stringResult);
    }
}