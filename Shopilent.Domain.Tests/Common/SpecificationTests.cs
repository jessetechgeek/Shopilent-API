using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Specifications;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Domain.Tests.Common;

public class SpecificationTests
{
    [Fact]
    public void AndSpecification_WithBothSatisfied_ShouldReturnTrue()
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

        var activeSpec = new ActiveProductSpecification();
        var priceSpec = new ProductPriceRangeSpecification(500, 1000);

        var andSpec = activeSpec.And(priceSpec);

        // Act
        var result = andSpec.IsSatisfiedBy(product);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AndSpecification_WithOneSatisfied_ShouldReturnFalse()
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

        var activeSpec = new ActiveProductSpecification();
        var priceSpec = new ProductPriceRangeSpecification(100, 500);

        var andSpec = activeSpec.And(priceSpec);

        // Act
        var result = andSpec.IsSatisfiedBy(product);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OrSpecification_WithBothSatisfied_ShouldReturnTrue()
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

        var activeSpec = new ActiveProductSpecification();
        var priceSpec = new ProductPriceRangeSpecification(500, 1000);

        var orSpec = activeSpec.Or(priceSpec);

        // Act
        var result = orSpec.IsSatisfiedBy(product);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OrSpecification_WithOneSatisfied_ShouldReturnTrue()
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

        var activeSpec = new ActiveProductSpecification();
        var priceSpec = new ProductPriceRangeSpecification(100, 500);

        var orSpec = activeSpec.Or(priceSpec);

        // Act
        var result = orSpec.IsSatisfiedBy(product);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OrSpecification_WithNeitherSatisfied_ShouldReturnFalse()
    {
        // Arrange
        var slugResult = Slug.Create("iphone");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        var priceResult = Money.FromDollars(999);
        Assert.True(priceResult.IsSuccess);
        var price = priceResult.Value;

        var productResult = Product.CreateInactive("iPhone", slug, price);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var activeSpec = new ActiveProductSpecification();
        var priceSpec = new ProductPriceRangeSpecification(100, 500);

        var orSpec = activeSpec.Or(priceSpec);

        // Act
        var result = orSpec.IsSatisfiedBy(product);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NotSpecification_WithSatisfied_ShouldReturnFalse()
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

        var activeSpec = new ActiveProductSpecification();
        var notSpec = activeSpec.Not();

        // Act
        var result = notSpec.IsSatisfiedBy(product);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NotSpecification_WithNotSatisfied_ShouldReturnTrue()
    {
        // Arrange
        var slugResult = Slug.Create("iphone");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        var priceResult = Money.FromDollars(999);
        Assert.True(priceResult.IsSuccess);
        var price = priceResult.Value;

        var productResult = Product.CreateInactive("iPhone", slug, price);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var activeSpec = new ActiveProductSpecification();
        var notSpec = activeSpec.Not();

        // Act
        var result = notSpec.IsSatisfiedBy(product);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ComplexSpecification_ShouldEvaluateCorrectly()
    {
        // Arrange - (Active OR ExpensivePrice) AND NOT InStockProduct
        var slugResult = Slug.Create("iphone");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        var priceResult = Money.FromDollars(999);
        Assert.True(priceResult.IsSuccess);
        var price = priceResult.Value;

        var productResult = Product.Create("iPhone", slug, price);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var variantResult = ProductVariant.CreateOutOfStock(product, "IP-BLK-128", price);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var addVariantResult = product.AddVariant(variant);
        Assert.True(addVariantResult.IsSuccess);

        var activeSpec = new ActiveProductSpecification();
        var priceSpec = new ProductPriceRangeSpecification(1500, 2000);
        var stockSpec = new InStockProductSpecification();

        var complexSpec = activeSpec.Or(priceSpec).And(stockSpec.Not());

        // Act
        var result = complexSpec.IsSatisfiedBy(product);

        // Assert
        Assert.True(result); // Active but not in stock, should match
    }
}