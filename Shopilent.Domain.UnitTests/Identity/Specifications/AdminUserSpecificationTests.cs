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
        var emailResult = Email.Create("admin@example.com");
        var fullNameResult = FullName.Create("Admin", "User");
        var userResult = User.CreateAdmin(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        var specification = new AdminUserSpecification();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithNonAdminUser_ShouldReturnFalse()
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
        var specification = new AdminUserSpecification();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithManagerUser_ShouldReturnFalse()
    {
        // Arrange
        var emailResult = Email.Create("manager@example.com");
        var fullNameResult = FullName.Create("Manager", "User");
        var userResult = User.CreateManager(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        var specification = new AdminUserSpecification();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }
}