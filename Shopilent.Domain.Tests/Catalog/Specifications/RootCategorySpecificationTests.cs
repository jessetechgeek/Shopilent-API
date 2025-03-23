using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Specifications;
using Shopilent.Domain.Catalog.ValueObjects;

namespace Shopilent.Domain.Tests.Catalog.Specifications;

public class RootCategorySpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WithRootCategory_ShouldReturnTrue()
    {
        // Arrange
        var slugResult = Slug.Create("electronics");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;
        
        var categoryResult = Category.Create("Electronics", slug);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        
        var specification = new RootCategorySpecification();

        // Act
        var result = specification.IsSatisfiedBy(category);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithChildCategory_ShouldReturnFalse()
    {
        // Arrange
        var parentSlugResult = Slug.Create("electronics");
        Assert.True(parentSlugResult.IsSuccess);
        var parentSlug = parentSlugResult.Value;
        
        var parentResult = Category.Create("Electronics", parentSlug);
        Assert.True(parentResult.IsSuccess);
        var parent = parentResult.Value;
        
        var childSlugResult = Slug.Create("phones");
        Assert.True(childSlugResult.IsSuccess);
        var childSlug = childSlugResult.Value;
        
        var childResult = Category.Create("Phones", childSlug, parent);
        Assert.True(childResult.IsSuccess);
        var child = childResult.Value;
        
        var specification = new RootCategorySpecification();

        // Act
        var result = specification.IsSatisfiedBy(child);

        // Assert
        Assert.False(result);
    }
}