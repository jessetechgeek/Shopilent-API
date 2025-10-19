using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Specifications;
using Shopilent.Domain.Catalog.ValueObjects;

namespace Shopilent.Domain.Tests.Catalog.Specifications;

public class ActiveCategorySpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WithActiveCategory_ShouldReturnTrue()
    {
        // Arrange
        var slugResult = Slug.Create("electronics");
        slugResult.IsSuccess.Should().BeTrue();
        var slug = slugResult.Value;
        
        var categoryResult = Category.Create("Electronics", slug);
        categoryResult.IsSuccess.Should().BeTrue();
        var category = categoryResult.Value;
        
        var specification = new ActiveCategorySpecification();

        // Act
        var result = specification.IsSatisfiedBy(category);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithInactiveCategory_ShouldReturnFalse()
    {
        // Arrange
        var slugResult = Slug.Create("electronics");
        slugResult.IsSuccess.Should().BeTrue();
        var slug = slugResult.Value;
        
        var categoryResult = Category.CreateInactive("Electronics", slug);
        categoryResult.IsSuccess.Should().BeTrue();
        var category = categoryResult.Value;
        
        var specification = new ActiveCategorySpecification();

        // Act
        var result = specification.IsSatisfiedBy(category);

        // Assert
        result.Should().BeFalse();
    }
}