using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Domain.Tests.Identity.ValueObjects;

public class PhoneNumberTests
{
    [Fact]
    public void Constructor_WithValidPhoneNumber_ShouldCreatePhoneNumber()
    {
        // Arrange
        var phoneNumberStr = "+1-555-123-4567";
        var expected = "+15551234567"; // Only digits and + retained

        // Act
        var phoneNumber = new PhoneNumber(phoneNumberStr);

        // Assert
        Assert.Equal(expected, phoneNumber.Value);
    }

    [Fact]
    public void Constructor_WithEmptyPhoneNumber_ShouldThrowArgumentException()
    {
        // Arrange
        var phoneNumberStr = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(phoneNumberStr));
        Assert.Equal("Phone number cannot be empty (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void Constructor_WithWhitespacePhoneNumber_ShouldThrowArgumentException()
    {
        // Arrange
        var phoneNumberStr = "   ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(phoneNumberStr));
        Assert.Equal("Phone number cannot be empty (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void Constructor_WithShortPhoneNumber_ShouldThrowArgumentException()
    {
        // Arrange
        var phoneNumberStr = "12345"; // Less than 7 digits

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(phoneNumberStr));
        Assert.Equal("Phone number is too short (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_ShouldRemoveThem()
    {
        // Arrange
        var phoneNumberStr = "(555) 123-4567";
        var expected = "5551234567";

        // Act
        var phoneNumber = new PhoneNumber(phoneNumberStr);

        // Assert
        Assert.Equal(expected, phoneNumber.Value);
    }

    [Fact]
    public void Constructor_WithLeadingPlus_ShouldPreserveIt()
    {
        // Arrange
        var phoneNumberStr = "+1 555 123 4567";
        var expected = "+15551234567";

        // Act
        var phoneNumber = new PhoneNumber(phoneNumberStr);

        // Assert
        Assert.Equal(expected, phoneNumber.Value);
    }

    [Fact]
    public void Constructor_WithLetters_ShouldRemoveThem()
    {
        // Arrange
        var phoneNumberStr = "555-CALL-NOW";
        var expected = "555"; // This would fail the length check in practice

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(phoneNumberStr));
        Assert.Equal("Phone number is too short (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void Constructor_WithValidFormattedNumber_ShouldNormalizeFormat()
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
            var phoneNumber = new PhoneNumber(number);
            if (number.StartsWith("+"))
            {
                Assert.Equal(expected, phoneNumber.Value);
            }
            else
            {
                Assert.Equal(expectedWithoutPlus, phoneNumber.Value);
            }
        }
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var phoneNumber1 = new PhoneNumber("+1-555-123-4567");
        var phoneNumber2 = new PhoneNumber("+1 (555) 123-4567");

        // Act & Assert
        Assert.True(phoneNumber1.Equals(phoneNumber2));
        Assert.True(phoneNumber1 == phoneNumber2);
        Assert.False(phoneNumber1 != phoneNumber2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var phoneNumber1 = new PhoneNumber("+1-555-123-4567");
        var phoneNumber2 = new PhoneNumber("+1-555-123-4568");

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
        var phoneNumber = new PhoneNumber(phoneNumberStr);

        // Act
        var result = phoneNumber.ToString();

        // Assert
        Assert.Equal(expected, result);
    }
}