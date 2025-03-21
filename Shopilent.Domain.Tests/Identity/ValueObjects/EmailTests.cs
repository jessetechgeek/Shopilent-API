using System;
using Xunit;
using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Domain.Tests.Identity.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldCreateEmail()
    {
        // Arrange
        var validEmails = new[]
        {
            "user@example.com",
            "first.last@example.com",
            "user+tag@example.com",
            "user@subdomain.example.com",
            "user123@example.co.uk"
        };

        foreach (var validEmail in validEmails)
        {
            // Act
            var email = Email.Create(validEmail);

            // Assert
            Assert.Equal(validEmail.ToLowerInvariant(), email.Value);
        }
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidEmails = new[]
        {
            "",
            "invalid",
            "invalid@",
            "@example.com",
            "user@.com",
            "user@example",
            "user@.example.com",
            "user@example..com",
            "user name@example.com",
            "user@example.com."
        };

        foreach (var invalidEmail in invalidEmails)
        {
            // Act & Assert
            if (string.IsNullOrWhiteSpace(invalidEmail))
            {
                var exception = Assert.Throws<ArgumentException>(() => Email.Create(invalidEmail));
                Assert.Equal("Email cannot be empty (Parameter 'value')", exception.Message);
            }
            else
            {
                var exception = Assert.Throws<ArgumentException>(() => Email.Create(invalidEmail));
                Assert.Equal("Invalid email format (Parameter 'value')", exception.Message);
            }
        }
    }

    [Fact]
    public void Create_WithMixedCaseEmail_ShouldNormalizeToLowercase()
    {
        // Arrange
        var mixedCaseEmail = "User.Name@Example.COM";
        var expectedEmail = "user.name@example.com";

        // Act
        var email = Email.Create(mixedCaseEmail);

        // Assert
        Assert.Equal(expectedEmail, email.Value);
    }

    [Fact]
    public void TryCreate_WithValidEmail_ShouldReturnTrueAndEmail()
    {
        // Arrange
        var validEmail = "user@example.com";

        // Act
        var result = Email.TryCreate(validEmail, out var email);

        // Assert
        Assert.True(result);
        Assert.NotNull(email);
        Assert.Equal(validEmail, email.Value);
    }

    [Fact]
    public void TryCreate_WithInvalidEmail_ShouldReturnFalseAndNullEmail()
    {
        // Arrange
        var invalidEmail = "invalid@@example.com";

        // Act
        var result = Email.TryCreate(invalidEmail, out var email);

        // Assert
        Assert.False(result);
        Assert.Null(email);
    }

    [Fact]
    public void Equals_WithSameEmail_ShouldReturnTrue()
    {
        // Arrange
        var email1 = Email.Create("user@example.com");
        var email2 = Email.Create("user@example.com");

        // Act & Assert
        Assert.True(email1.Equals(email2));
        Assert.True(email1 == email2);
        Assert.False(email1 != email2);
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email1 = Email.Create("user1@example.com");
        var email2 = Email.Create("user2@example.com");

        // Act & Assert
        Assert.False(email1.Equals(email2));
        Assert.False(email1 == email2);
        Assert.True(email1 != email2);
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var emailValue = "user@example.com";
        var email = Email.Create(emailValue);

        // Act
        var result = email.ToString();

        // Assert
        Assert.Equal(emailValue, result);
    }
}