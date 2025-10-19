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
        var emailResult = Email.Create("test@example.com");
        var fullNameResult = FullName.Create("John", "Doe");
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        var specification = new ActiveUserSpecification();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithInactiveUser_ShouldReturnFalse()
    {
        // Arrange
        var emailResult = Email.Create("test@example.com");
        var fullNameResult = FullName.Create("John", "Doe");
        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);
            
        userResult.IsSuccess.Should().BeTrue();
        var user = userResult.Value;
        var deactivateResult = user.Deactivate();
        deactivateResult.IsSuccess.Should().BeTrue();
        
        var specification = new ActiveUserSpecification();

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }
}