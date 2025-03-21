using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Domain.Tests.Identity.ValueObjects;

public class FullNameTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateFullName()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var middleName = "Smith";

        // Act
        var fullName = new FullName(firstName, lastName, middleName);

        // Assert
        Assert.Equal(firstName, fullName.FirstName);
        Assert.Equal(lastName, fullName.LastName);
        Assert.Equal(middleName, fullName.MiddleName);
    }

    [Fact]
    public void Constructor_WithoutMiddleName_ShouldCreateFullName()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var fullName = new FullName(firstName, lastName);

        // Assert
        Assert.Equal(firstName, fullName.FirstName);
        Assert.Equal(lastName, fullName.LastName);
        Assert.Null(fullName.MiddleName);
    }

    [Fact]
    public void Constructor_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        // Arrange
        var firstName = string.Empty;
        var lastName = "Doe";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FullName(firstName, lastName));
        Assert.Equal("First name cannot be empty (Parameter 'firstName')", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyLastName_ShouldThrowArgumentException()
    {
        // Arrange
        var firstName = "John";
        var lastName = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FullName(firstName, lastName));
        Assert.Equal("Last name cannot be empty (Parameter 'lastName')", exception.Message);
    }

    [Fact]
    public void ToString_WithMiddleName_ShouldIncludeAllParts()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var middleName = "Smith";
        var fullName = new FullName(firstName, lastName, middleName);
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
        var fullName = new FullName(firstName, lastName);
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
        var fullName1 = new FullName("John", "Doe", "Smith");
        var fullName2 = new FullName("John", "Doe", "Smith");

        // Act & Assert
        Assert.True(fullName1.Equals(fullName2));
        Assert.True(fullName1 == fullName2);
        Assert.False(fullName1 != fullName2);
    }

    [Fact]
    public void Equals_WithDifferentFirstName_ShouldReturnFalse()
    {
        // Arrange
        var fullName1 = new FullName("John", "Doe", "Smith");
        var fullName2 = new FullName("Jane", "Doe", "Smith");

        // Act & Assert
        Assert.False(fullName1.Equals(fullName2));
        Assert.False(fullName1 == fullName2);
        Assert.True(fullName1 != fullName2);
    }

    [Fact]
    public void Equals_WithDifferentLastName_ShouldReturnFalse()
    {
        // Arrange
        var fullName1 = new FullName("John", "Doe", "Smith");
        var fullName2 = new FullName("John", "Smith", "Smith");

        // Act & Assert
        Assert.False(fullName1.Equals(fullName2));
        Assert.False(fullName1 == fullName2);
        Assert.True(fullName1 != fullName2);
    }

    [Fact]
    public void Equals_WithDifferentMiddleName_ShouldReturnFalse()
    {
        // Arrange
        var fullName1 = new FullName("John", "Doe", "Smith");
        var fullName2 = new FullName("John", "Doe", "Jones");

        // Act & Assert
        Assert.False(fullName1.Equals(fullName2));
        Assert.False(fullName1 == fullName2);
        Assert.True(fullName1 != fullName2);
    }

    [Fact]
    public void Equals_WithNullMiddleName_ShouldHandleComparisonCorrectly()
    {
        // Arrange
        var fullName1 = new FullName("John", "Doe");
        var fullName2 = new FullName("John", "Doe");
        var fullName3 = new FullName("John", "Doe", "Smith");

        // Act & Assert
        Assert.True(fullName1.Equals(fullName2));
        Assert.False(fullName1.Equals(fullName3));
    }
}