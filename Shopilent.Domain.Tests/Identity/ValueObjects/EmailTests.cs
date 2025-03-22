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
            var result = Email.Create(validEmail);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(validEmail.ToLowerInvariant(), result.Value.Value);
        }
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldReturnFailure()
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
            // Act
            var result = Email.Create(invalidEmail);

            // Assert
            if (string.IsNullOrWhiteSpace(invalidEmail))
            {
                Assert.True(result.IsFailure);
                Assert.Equal("User.EmailRequired", result.Error.Code);
            }
            else
            {
                Assert.True(result.IsFailure);
                Assert.Equal("User.InvalidEmailFormat", result.Error.Code);
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
        var result = Email.Create(mixedCaseEmail);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedEmail, result.Value.Value);
    }

    [Fact]
    public void TryCreate_WithValidEmail_ShouldReturnSuccessAndEmail()
    {
        // Arrange
        var validEmail = "user@example.com";

        // Act
        var result = Email.TryCreate(validEmail);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(validEmail, result.Value.Value);
    }

    [Fact]
    public void TryCreate_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var invalidEmail = "invalid@@example.com";

        // Act
        var result = Email.TryCreate(invalidEmail);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.InvalidEmailFormat", result.Error.Code);
    }

    [Fact]
    public void Equals_WithSameEmail_ShouldReturnTrue()
    {
        // Arrange
        var email1Result = Email.Create("user@example.com");
        var email2Result = Email.Create("user@example.com");
        
        Assert.True(email1Result.IsSuccess);
        Assert.True(email2Result.IsSuccess);
        
        var email1 = email1Result.Value;
        var email2 = email2Result.Value;

        // Act & Assert
        Assert.True(email1.Equals(email2));
        Assert.True(email1 == email2);
        Assert.False(email1 != email2);
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email1Result = Email.Create("user1@example.com");
        var email2Result = Email.Create("user2@example.com");
        
        Assert.True(email1Result.IsSuccess);
        Assert.True(email2Result.IsSuccess);
        
        var email1 = email1Result.Value;
        var email2 = email2Result.Value;

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
        var emailResult = Email.Create(emailValue);
        Assert.True(emailResult.IsSuccess);
        var email = emailResult.Value;

        // Act
        var result = email.ToString();

        // Assert
        Assert.Equal(emailValue, result);
    }
}