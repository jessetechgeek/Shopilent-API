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
            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be(validEmail.ToLowerInvariant());
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
                result.IsFailure.Should().BeTrue();
                result.Error.Code.Should().Be("User.EmailRequired");
            }
            else
            {
                result.IsFailure.Should().BeTrue();
                result.Error.Code.Should().Be("User.InvalidEmailFormat");
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
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expectedEmail);
    }

    [Fact]
    public void TryCreate_WithValidEmail_ShouldReturnSuccessAndEmail()
    {
        // Arrange
        var validEmail = "user@example.com";

        // Act
        var result = Email.TryCreate(validEmail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(validEmail);
    }

    [Fact]
    public void TryCreate_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var invalidEmail = "invalid@@example.com";

        // Act
        var result = Email.TryCreate(invalidEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidEmailFormat");
    }

    [Fact]
    public void Equals_WithSameEmail_ShouldReturnTrue()
    {
        // Arrange
        var email1Result = Email.Create("user@example.com");
        var email2Result = Email.Create("user@example.com");
        
        email1Result.IsSuccess.Should().BeTrue();
        email2Result.IsSuccess.Should().BeTrue();
        
        var email1 = email1Result.Value;
        var email2 = email2Result.Value;

        // Act & Assert
        email1.Equals(email2).Should().BeTrue();
        (email1 == email2).Should().BeTrue();
        (email1 != email2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email1Result = Email.Create("user1@example.com");
        var email2Result = Email.Create("user2@example.com");
        
        email1Result.IsSuccess.Should().BeTrue();
        email2Result.IsSuccess.Should().BeTrue();
        
        var email1 = email1Result.Value;
        var email2 = email2Result.Value;

        // Act & Assert
        email1.Equals(email2).Should().BeFalse();
        (email1 == email2).Should().BeFalse();
        (email1 != email2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var emailValue = "user@example.com";
        var emailResult = Email.Create(emailValue);
        emailResult.IsSuccess.Should().BeTrue();
        var email = emailResult.Value;

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be(emailValue);
    }
}