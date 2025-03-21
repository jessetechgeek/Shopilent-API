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
        var user = User.CreatePreVerified(
            Email.Create("verified@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
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
        var user = User.Create(
            Email.Create("unverified@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
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
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));

        // Initial check
        var specification = new VerifiedUserSpecification();
        Assert.False(specification.IsSatisfiedBy(user));

        // Verify the user
        user.VerifyEmail();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        Assert.True(result);
    }
}