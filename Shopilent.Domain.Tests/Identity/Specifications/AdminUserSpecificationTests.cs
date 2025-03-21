using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.Specifications;
using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Domain.Tests.Identity.Specifications;

public class AdminUserSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WithAdminUser_ShouldReturnTrue()
    {
        // Arrange
        var user = User.CreateAdmin(
            Email.Create("admin@example.com"),
            "hashed_password",
            new FullName("Admin", "User"));
        var specification = new AdminUserSpecification();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithNonAdminUser_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Create(
            Email.Create("user@example.com"),
            "hashed_password",
            new FullName("John", "Doe"));
        var specification = new AdminUserSpecification();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithManagerUser_ShouldReturnFalse()
    {
        // Arrange
        var user = User.CreateManager(
            Email.Create("manager@example.com"),
            "hashed_password",
            new FullName("Manager", "User"));

        var specification = new AdminUserSpecification();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        Assert.False(result);
    }
}