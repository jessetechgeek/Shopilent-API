using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.Specifications;
using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Domain.Tests.Identity.Specifications;

public class ActiveUserSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WithActiveUser_ShouldReturnTrue()
    {
        // Arrange
        var user = User.Create(
            Email.Create("test@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        var specification = new ActiveUserSpecification();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithInactiveUser_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Create(
            Email.Create("test@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        user.Deactivate();
        var specification = new ActiveUserSpecification();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        Assert.False(result);
    }
}