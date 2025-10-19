using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.Specifications;
using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Domain.Tests.Identity.Specifications;

public class VerifiedUserSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WithVerifiedUser_ShouldReturnTrue()
    {
        // Arrange
        var emailResult = Email.Create("verified@example.com");
        var fullNameResult = FullName.Create("John", "Doe");
        var userResult = User.CreatePreVerified(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        var specification = new VerifiedUserSpecification();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithUnverifiedUser_ShouldReturnFalse()
    {
        // Arrange
        var emailResult = Email.Create("unverified@example.com");
        var fullNameResult = FullName.Create("John", "Doe");
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        var specification = new VerifiedUserSpecification();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithUserWhoGetsVerified_ShouldReturnTrue()
    {
        // Arrange
        var emailResult = Email.Create("user@example.com");
        var fullNameResult = FullName.Create("John", "Doe");
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;

        // Initial check
        var specification = new VerifiedUserSpecification();
        specification.IsSatisfiedBy(user).Should().BeFalse();

        // Verify the user
        var verifyResult = user.VerifyEmail();
        verifyResult.IsSuccess.Should().BeTrue();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }
}