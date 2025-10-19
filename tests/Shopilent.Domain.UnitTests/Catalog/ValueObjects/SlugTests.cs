using Shopilent.Domain.Catalog.ValueObjects;

namespace Shopilent.Domain.Tests.Catalog.ValueObjects;

public class SlugTests
{
    [Fact]
    public void Create_WithValidInput_ShouldCreateSlug()
    {
        // Arrange
        var value = "test-slug";

        // Act
        var result = Slug.Create(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithEmptyInput_ShouldReturnFailure()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var result = Slug.Create(value);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithWhitespaceInput_ShouldReturnFailure()
    {
        // Arrange
        var value = "   ";

        // Act
        var result = Slug.Create(value);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithSpaces_ShouldReplaceWithHyphens()
    {
        // Arrange
        var input = "This is a test slug";
        var expected = "this-is-a-test-slug";

        // Act
        var result = Slug.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
    }

    [Fact]
    public void Create_WithUppercase_ShouldConvertToLowercase()
    {
        // Arrange
        var input = "THIS-IS-A-TEST-SLUG";
        var expected = "this-is-a-test-slug";

        // Act
        var result = Slug.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
    }

    [Fact]
    public void Create_WithSpecialCharacters_ShouldRemoveThem()
    {
        // Arrange
        var input = "product!@#$%^&*()_+name";
        var expected = "productname";

        // Act
        var result = Slug.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
    }

    [Fact]
    public void Create_WithMultipleHyphens_ShouldNormalizeThem()
    {
        // Arrange
        var input = "product----name";
        var expected = "product-name";

        // Act
        var result = Slug.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var slug1Result = Slug.Create("test-slug");
        var slug2Result = Slug.Create("test-slug");
        
        slug1Result.IsSuccess.Should().BeTrue();
        slug2Result.IsSuccess.Should().BeTrue();
        
        var slug1 = slug1Result.Value;
        var slug2 = slug2Result.Value;

        // Act & Assert
        slug1.Equals(slug2).Should().BeTrue();
        (slug1 == slug2).Should().BeTrue();
        (slug1 != slug2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var slug1Result = Slug.Create("test-slug-1");
        var slug2Result = Slug.Create("test-slug-2");
        
        slug1Result.IsSuccess.Should().BeTrue();
        slug2Result.IsSuccess.Should().BeTrue();
        
        var slug1 = slug1Result.Value;
        var slug2 = slug2Result.Value;

        // Act & Assert
        slug1.Equals(slug2).Should().BeFalse();
        (slug1 == slug2).Should().BeFalse();
        (slug1 != slug2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var value = "test-slug";
        var slugResult = Slug.Create(value);
        slugResult.IsSuccess.Should().BeTrue();
        var slug = slugResult.Value;

        // Act
        var result = slug.ToString();

        // Assert
        result.Should().Be(value);
    }
}