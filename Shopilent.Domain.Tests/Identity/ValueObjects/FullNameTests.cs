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
        Assert.True(result.IsSuccess);
        var fullName = result.Value;
        Assert.Equal(firstName, fullName.FirstName);
        Assert.Equal(lastName, fullName.LastName);
        Assert.Equal(middleName, fullName.MiddleName);
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
        Assert.True(result.IsSuccess);
        var fullName = result.Value;
        Assert.Equal(firstName, fullName.FirstName);
        Assert.Equal(lastName, fullName.LastName);
        Assert.Null(fullName.MiddleName);
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
        Assert.True(result.IsFailure);
        Assert.Equal("User.FirstNameRequired", result.Error.Code);
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
        Assert.True(result.IsFailure);
        Assert.Equal("User.LastNameRequired", result.Error.Code);
    }

    [Fact]
    public void ToString_WithMiddleName_ShouldIncludeAllParts()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var middleName = "Smith";
        var fullNameResult = FullName.Create(firstName, lastName, middleName);
        Assert.True(fullNameResult.IsSuccess);
        var fullName = fullNameResult.Value;
        var expected = "John Smith Doe";

        // Act
        var result = fullName.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToString_WithoutMiddleName_ShouldExcludeMiddleName()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var fullNameResult = FullName.Create(firstName, lastName);
        Assert.True(fullNameResult.IsSuccess);
        var fullName = fullNameResult.Value;
        var expected = "John Doe";

        // Act
        var result = fullName.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var fullName1Result = FullName.Create("John", "Doe", "Smith");
        var fullName2Result = FullName.Create("John", "Doe", "Smith");
        
        Assert.True(fullName1Result.IsSuccess);
        Assert.True(fullName2Result.IsSuccess);
        
        var fullName1 = fullName1Result.Value;
        var fullName2 = fullName2Result.Value;

        // Act & Assert
        Assert.True(fullName1.Equals(fullName2));
        Assert.True(fullName1 == fullName2);
        Assert.False(fullName1 != fullName2);
    }

    [Fact]
    public void Equals_WithDifferentFirstName_ShouldReturnFalse()
    {
        // Arrange
        var fullName1Result = FullName.Create("John", "Doe", "Smith");
        var fullName2Result = FullName.Create("Jane", "Doe", "Smith");
        
        Assert.True(fullName1Result.IsSuccess);
        Assert.True(fullName2Result.IsSuccess);
        
        var fullName1 = fullName1Result.Value;
        var fullName2 = fullName2Result.Value;

        // Act & Assert
        Assert.False(fullName1.Equals(fullName2));
        Assert.False(fullName1 == fullName2);
        Assert.True(fullName1 != fullName2);
    }

    [Fact]
    public void Equals_WithDifferentLastName_ShouldReturnFalse()
    {
        // Arrange
        var fullName1Result = FullName.Create("John", "Doe", "Smith");
        var fullName2Result = FullName.Create("John", "Smith", "Smith");
        
        Assert.True(fullName1Result.IsSuccess);
        Assert.True(fullName2Result.IsSuccess);
        
        var fullName1 = fullName1Result.Value;
        var fullName2 = fullName2Result.Value;

        // Act & Assert
        Assert.False(fullName1.Equals(fullName2));
        Assert.False(fullName1 == fullName2);
        Assert.True(fullName1 != fullName2);
    }

    [Fact]
    public void Equals_WithDifferentMiddleName_ShouldReturnFalse()
    {
        // Arrange
        var fullName1Result = FullName.Create("John", "Doe", "Smith");
        var fullName2Result = FullName.Create("John", "Doe", "Jones");
        
        Assert.True(fullName1Result.IsSuccess);
        Assert.True(fullName2Result.IsSuccess);
        
        var fullName1 = fullName1Result.Value;
        var fullName2 = fullName2Result.Value;

        // Act & Assert
        Assert.False(fullName1.Equals(fullName2));
        Assert.False(fullName1 == fullName2);
        Assert.True(fullName1 != fullName2);
    }

    [Fact]
    public void Equals_WithNullMiddleName_ShouldHandleComparisonCorrectly()
    {
        // Arrange
        var fullName1Result = FullName.Create("John", "Doe");
        var fullName2Result = FullName.Create("John", "Doe");
        var fullName3Result = FullName.Create("John", "Doe", "Smith");
        
        Assert.True(fullName1Result.IsSuccess);
        Assert.True(fullName2Result.IsSuccess);
        Assert.True(fullName3Result.IsSuccess);
        
        var fullName1 = fullName1Result.Value;
        var fullName2 = fullName2Result.Value;
        var fullName3 = fullName3Result.Value;

        // Act & Assert
        Assert.True(fullName1.Equals(fullName2));
        Assert.False(fullName1.Equals(fullName3));
    }
}