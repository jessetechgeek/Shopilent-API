using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Specifications;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Domain.Tests.Catalog.Specifications;

public class ProductInCategorySpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WithProductInCategory_ShouldReturnTrue()
    {
        // Arrange
        var categorySlugResult = Slug.Create("electronics");
        Assert.True(categorySlugResult.IsSuccess);
        var categorySlug = categorySlugResult.Value;
        
        var categoryResult = Category.Create("Electronics", categorySlug);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        
        var productSlugResult = Slug.Create("iphone");
        Assert.True(productSlugResult.IsSuccess);
        var productSlug = productSlugResult.Value;
        
        var priceResult = Money.FromDollars(999);
        Assert.True(priceResult.IsSuccess);
        var price = priceResult.Value;
        
        var productResult = Product.Create("iPhone", productSlug, price);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        
        var addCategoryResult = product.AddCategory(category);
        Assert.True(addCategoryResult.IsSuccess);

        var specification = new ProductInCategorySpecification(category.Id);

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithProductNotInCategory_ShouldReturnFalse()
    {
        // Arrange
        var categorySlugResult = Slug.Create("electronics");
        Assert.True(categorySlugResult.IsSuccess);
        var categorySlug = categorySlugResult.Value;
        
        var categoryResult = Category.Create("Electronics", categorySlug);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        
        var otherCategorySlugResult = Slug.Create("phones");
        Assert.True(otherCategorySlugResult.IsSuccess);
        var otherCategorySlug = otherCategorySlugResult.Value;
        
        var otherCategoryResult = Category.Create("Phones", otherCategorySlug);
        Assert.True(otherCategoryResult.IsSuccess);
        var otherCategory = otherCategoryResult.Value;
        
        var productSlugResult = Slug.Create("iphone");
        Assert.True(productSlugResult.IsSuccess);
        var productSlug = productSlugResult.Value;
        
        var priceResult = Money.FromDollars(999);
        Assert.True(priceResult.IsSuccess);
        var price = priceResult.Value;
        
        var productResult = Product.Create("iPhone", productSlug, price);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        
        var addCategoryResult = product.AddCategory(otherCategory);
        Assert.True(addCategoryResult.IsSuccess);

        var specification = new ProductInCategorySpecification(category.Id);

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithProductInMultipleCategories_ShouldReturnTrue()
    {
        // Arrange
        var category1SlugResult = Slug.Create("electronics");
        Assert.True(category1SlugResult.IsSuccess);
        var category1Slug = category1SlugResult.Value;
        
        var category1Result = Category.Create("Electronics", category1Slug);
        Assert.True(category1Result.IsSuccess);
        var category1 = category1Result.Value;
        
        var category2SlugResult = Slug.Create("phones");
        Assert.True(category2SlugResult.IsSuccess);
        var category2Slug = category2SlugResult.Value;
        
        var category2Result = Category.Create("Phones", category2Slug);
        Assert.True(category2Result.IsSuccess);
        var category2 = category2Result.Value;
        
        var productSlugResult = Slug.Create("iphone");
        Assert.True(productSlugResult.IsSuccess);
        var productSlug = productSlugResult.Value;
        
        var priceResult = Money.FromDollars(999);
        Assert.True(priceResult.IsSuccess);
        var price = priceResult.Value;
        
        var productResult = Product.Create("iPhone", productSlug, price);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        product.AddCategory(category1);
        product.AddCategory(category2);

        var specification = new ProductInCategorySpecification(category1.Id);

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        Assert.True(result);
    }
}