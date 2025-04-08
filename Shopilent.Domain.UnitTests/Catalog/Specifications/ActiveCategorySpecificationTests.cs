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
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;
        
        var categoryResult = Category.Create("Electronics", slug);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        
        var specification = new ActiveCategorySpecification();

        // Act
        var result = specification.IsSatisfiedBy(category);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithInactiveCategory_ShouldReturnFalse()
    {
        // Arrange
        var slugResult = Slug.Create("electronics");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;
        
        var categoryResult = Category.CreateInactive("Electronics", slug);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        
        var specification = new ActiveCategorySpecification();

        // Act
        var result = specification.IsSatisfiedBy(category);

        // Assert
        Assert.False(result);
    }
}