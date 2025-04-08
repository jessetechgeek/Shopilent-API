using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Specifications;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Domain.Tests.Catalog.Specifications;

public class InStockProductSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WithProductWithoutVariants_ShouldReturnTrue()
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

        var specification = new InStockProductSpecification();

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithProductWithInStockVariants_ShouldReturnTrue()
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

        var variantResult = ProductVariant.Create(product.Id, "IP-BLK-64", price, 10);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var addVariantResult = product.AddVariant(variant);
        Assert.True(addVariantResult.IsSuccess);

        var specification = new InStockProductSpecification();

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithProductWithOutOfStockVariants_ShouldReturnFalse()
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

        var variantResult = ProductVariant.CreateOutOfStock(product, "IP-BLK-64", price);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var addVariantResult = product.AddVariant(variant);
        Assert.True(addVariantResult.IsSuccess);

        var specification = new InStockProductSpecification();

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithProductWithMixedStockVariants_ShouldReturnTrue()
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

        var outOfStockVariantResult = ProductVariant.CreateOutOfStock(product, "IP-BLK-64", price);
        Assert.True(outOfStockVariantResult.IsSuccess);
        var outOfStockVariant = outOfStockVariantResult.Value;

        var inStockVariantResult = ProductVariant.Create(product.Id, "IP-WHT-64", price, 5);
        Assert.True(inStockVariantResult.IsSuccess);
        var inStockVariant = inStockVariantResult.Value;

        product.AddVariant(outOfStockVariant);
        product.AddVariant(inStockVariant);

        var specification = new InStockProductSpecification();

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_WithProductWithInactiveVariants_ShouldReturnFalse()
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

        var variantResult = ProductVariant.Create(product.Id, "IP-BLK-64", price, 10);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var deactivateResult = variant.Deactivate();
        Assert.True(deactivateResult.IsSuccess);

        var addVariantResult = product.AddVariant(variant);
        Assert.True(addVariantResult.IsSuccess);

        var specification = new InStockProductSpecification();

        // Act
        var result = specification.IsSatisfiedBy(product);

        // Assert
        Assert.False(result);
    }
}