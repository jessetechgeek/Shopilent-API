using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Domain.Tests.Identity.ValueObjects;

public class FullNameTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateFullName()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var middleName = "Smith";

        // Act
        var result = FullName.Create(firstName, lastName, middleName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var fullName = result.Value;
        fullName.FirstName.Should().Be(firstName);
        fullName.LastName.Should().Be(lastName);
        fullName.MiddleName.Should().Be(middleName);
    }

    [Fact]
    public void Create_WithoutMiddleName_ShouldCreateFullName()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var result = FullName.Create(firstName, lastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var fullName = result.Value;
        fullName.FirstName.Should().Be(firstName);
        fullName.LastName.Should().Be(lastName);
        fullName.MiddleName.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyFirstName_ShouldReturnFailure()
    {
        // Arrange
        var firstName = string.Empty;
        var lastName = "Doe";

        // Act
        var result = FullName.Create(firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.FirstNameRequired");
    }

    [Fact]
    public void Create_WithEmptyLastName_ShouldReturnFailure()
    {
        // Arrange
        var firstName = "John";
        var lastName = string.Empty;

        // Act
        var result = FullName.Create(firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.LastNameRequired");
    }

    [Fact]
    public void ToString_WithMiddleName_ShouldIncludeAllParts()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var middleName = "Smith";
        var fullNameResult = FullName.Create(firstName, lastName, middleName);
        fullNameResult.IsSuccess.Should().BeTrue();
        var fullName = fullNameResult.Value;
        var expected = "John Smith Doe";

        // Act
        var result = fullName.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToString_WithoutMiddleName_ShouldExcludeMiddleName()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var fullNameResult = FullName.Create(firstName, lastName);
        fullNameResult.IsSuccess.Should().BeTrue();
        var fullName = fullNameResult.Value;
        var expected = "John Doe";

        // Act
        var result = fullName.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var fullName1Result = FullName.Create("John", "Doe", "Smith");
        var fullName2Result = FullName.Create("John", "Doe", "Smith");
        
        fullName1Result.IsSuccess.Should().BeTrue();
        fullName2Result.IsSuccess.Should().BeTrue();
        
        var fullName1 = fullName1Result.Value;
        var fullName2 = fullName2Result.Value;

        // Act & Assert
        fullName1.Equals(fullName2).Should().BeTrue();
        (fullName1 == fullName2).Should().BeTrue();
        (fullName1 != fullName2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentFirstName_ShouldReturnFalse()
    {
        // Arrange
        var fullName1Result = FullName.Create("John", "Doe", "Smith");
        var fullName2Result = FullName.Create("Jane", "Doe", "Smith");
        
        fullName1Result.IsSuccess.Should().BeTrue();
        fullName2Result.IsSuccess.Should().BeTrue();
        
        var fullName1 = fullName1Result.Value;
        var fullName2 = fullName2Result.Value;

        // Act & Assert
        fullName1.Equals(fullName2).Should().BeFalse();
        (fullName1 == fullName2).Should().BeFalse();
        (fullName1 != fullName2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentLastName_ShouldReturnFalse()
    {
        // Arrange
        var fullName1Result = FullName.Create("John", "Doe", "Smith");
        var fullName2Result = FullName.Create("John", "Smith", "Smith");
        
        fullName1Result.IsSuccess.Should().BeTrue();
        fullName2Result.IsSuccess.Should().BeTrue();
        
        var fullName1 = fullName1Result.Value;
        var fullName2 = fullName2Result.Value;

        // Act & Assert
        fullName1.Equals(fullName2).Should().BeFalse();
        (fullName1 == fullName2).Should().BeFalse();
        (fullName1 != fullName2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentMiddleName_ShouldReturnFalse()
    {
        // Arrange
        var fullName1Result = FullName.Create("John", "Doe", "Smith");
        var fullName2Result = FullName.Create("John", "Doe", "Jones");
        
        fullName1Result.IsSuccess.Should().BeTrue();
        fullName2Result.IsSuccess.Should().BeTrue();
        
        var fullName1 = fullName1Result.Value;
        var fullName2 = fullName2Result.Value;

        // Act & Assert
        fullName1.Equals(fullName2).Should().BeFalse();
        (fullName1 == fullName2).Should().BeFalse();
        (fullName1 != fullName2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNullMiddleName_ShouldHandleComparisonCorrectly()
    {
        // Arrange
        var fullName1Result = FullName.Create("John", "Doe");
        var fullName2Result = FullName.Create("John", "Doe");
        var fullName3Result = FullName.Create("John", "Doe", "Smith");
        
        fullName1Result.IsSuccess.Should().BeTrue();
        fullName2Result.IsSuccess.Should().BeTrue();
        fullName3Result.IsSuccess.Should().BeTrue();
        
        var fullName1 = fullName1Result.Value;
        var fullName2 = fullName2Result.Value;
        var fullName3 = fullName3Result.Value;

        // Act & Assert
        fullName1.Equals(fullName2).Should().BeTrue();
        fullName1.Equals(fullName3).Should().BeFalse();
    }
}