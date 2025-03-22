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
            
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        var specification = new VerifiedUserSpecification();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        Assert.True(result);
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
            
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;
        var specification = new VerifiedUserSpecification();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        Assert.False(result);
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
            
        Assert.True(userResult.IsSuccess);
        var user = userResult.Value;

        // Initial check
        var specification = new VerifiedUserSpecification();
        Assert.False(specification.IsSatisfiedBy(user));

        // Verify the user
        var verifyResult = user.VerifyEmail();
        Assert.True(verifyResult.IsSuccess);

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        Assert.True(result);
    }
}