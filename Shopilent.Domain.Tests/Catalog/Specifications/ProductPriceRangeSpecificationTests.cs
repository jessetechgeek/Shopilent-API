using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Specifications;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Domain.Tests.Catalog.Specifications;

public class ProductPriceRangeSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WithPriceInRange_ShouldReturnTrue()
    {
        // Arrange
        var slugResult = Slug.Create("iphone");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;
        
        var priceResult = Money.FromDollars(999);
        Assert.True(priceResult.IsSuccess);
        var price = priceResult.Value;
        
        var productResult = Product.Create("iPhone", slug, price);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        
        var specification = new ProductPriceRangeSpecification(500, 1000);

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithPriceAtLowerBound_ShouldReturnTrue()
    {
        // Arrange
        var slugResult = Slug.Create("iphone");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;
        
        var priceResult = Money.FromDollars(500);
        Assert.True(priceResult.IsSuccess);
        var price = priceResult.Value;
        
        var productResult = Product.Create("iPhone", slug, price);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        
        var specification = new ProductPriceRangeSpecification(500, 1000);

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithPriceAtUpperBound_ShouldReturnTrue()
    {
        // Arrange
        var slugResult = Slug.Create("iphone");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;
        
        var priceResult = Money.FromDollars(1000);
        Assert.True(priceResult.IsSuccess);
        var price = priceResult.Value;
        
        var productResult = Product.Create("iPhone", slug, price);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        
        var specification = new ProductPriceRangeSpecification(500, 1000);

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithPriceBelowRange_ShouldReturnFalse()
    {
        // Arrange
        var slugResult = Slug.Create("iphone");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;
        
        var priceResult = Money.FromDollars(499);
        Assert.True(priceResult.IsSuccess);
        var price = priceResult.Value;
        
        var productResult = Product.Create("iPhone", slug, price);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        
        var specification = new ProductPriceRangeSpecification(500, 1000);

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithPriceAboveRange_ShouldReturnFalse()
    {
        // Arrange
        var slugResult = Slug.Create("iphone");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;
        
        var priceResult = Money.FromDollars(1001);
        Assert.True(priceResult.IsSuccess);
        var price = priceResult.Value;
        
        var productResult = Product.Create("iPhone", slug, price);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        
        var specification = new ProductPriceRangeSpecification(500, 1000);

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        Assert.False(result);
    }
}