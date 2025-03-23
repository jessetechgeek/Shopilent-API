using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Enums;
using Shopilent.Domain.Catalog.Events;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Sales.ValueObjects;
using Attribute = Shopilent.Domain.Catalog.Attribute;

namespace Shopilent.Domain.Tests.Catalog;

public class ProductTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateProduct()
    {
        // Arrange
        var name = "iPhone 13";
        var slugResult = Slug.Create("iphone-13");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        var basePriceResult = Money.FromDollars(999);
        Assert.True(basePriceResult.IsSuccess);
        var basePrice = basePriceResult.Value;

        var sku = "IP13-64GB";

        // Act
        var result = Product.Create(name, slug, basePrice, sku);

        // Assert
        Assert.True(result.IsSuccess);
        var product = result.Value;
        Assert.Equal(name, product.Name);
        Assert.Equal(slug, product.Slug);
        Assert.Equal(basePrice, product.BasePrice);
        Assert.Equal(sku, product.Sku);
        Assert.True(product.IsActive);
        Assert.Empty(product.Categories);
        Assert.Empty(product.Attributes);
        Assert.Empty(product.Variants);
        Assert.Contains(product.DomainEvents, e => e is ProductCreatedEvent);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var name = string.Empty;
        var slugResult = Slug.Create("iphone-13");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        var basePriceResult = Money.FromDollars(999);
        Assert.True(basePriceResult.IsSuccess);
        var basePrice = basePriceResult.Value;

        // Act
        var result = Product.Create(name, slug, basePrice);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Product.NameRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithEmptySlug_ShouldReturnFailure()
    {
        // Arrange
        var name = "iPhone 13";
        var slugResult = Slug.Create(string.Empty);

        // Act & Assert
        Assert.True(slugResult.IsFailure);
        Assert.Equal("Category.SlugRequired", slugResult.Error.Code);
    }

    [Fact]
    public void Create_WithNullBasePrice_ShouldReturnFailure()
    {
        // Arrange
        var name = "iPhone 13";
        var slugResult = Slug.Create("iphone-13");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        Money basePrice = null;

        // Act
        var result = Product.Create(name, slug, basePrice);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Product.NegativePrice", result.Error.Code);
    }

    [Fact]
    public void CreateWithDescription_ShouldCreateProductWithDescription()
    {
        // Arrange
        var name = "iPhone 13";
        var slugResult = Slug.Create("iphone-13");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        var basePriceResult = Money.FromDollars(999);
        Assert.True(basePriceResult.IsSuccess);
        var basePrice = basePriceResult.Value;

        var description = "Latest iPhone with A15 Bionic chip";

        // Act
        var result = Product.CreateWithDescription(name, slug, basePrice, description);

        // Assert
        Assert.True(result.IsSuccess);
        var product = result.Value;
        Assert.Equal(name, product.Name);
        Assert.Equal(slug, product.Slug);
        Assert.Equal(basePrice, product.BasePrice);
        Assert.Equal(description, product.Description);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void CreateInactive_ShouldCreateInactiveProduct()
    {
        // Arrange
        var name = "iPhone 13";
        var slugResult = Slug.Create("iphone-13");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        var basePriceResult = Money.FromDollars(999);
        Assert.True(basePriceResult.IsSuccess);
        var basePrice = basePriceResult.Value;

        // Act
        var result = Product.CreateInactive(name, slug, basePrice);

        // Assert
        Assert.True(result.IsSuccess);
        var product = result.Value;
        Assert.Equal(name, product.Name);
        Assert.Equal(slug, product.Slug);
        Assert.Equal(basePrice, product.BasePrice);
        Assert.False(product.IsActive);
        Assert.Contains(product.DomainEvents, e => e is ProductCreatedEvent);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateProduct()
    {
        // Arrange
        var productResult = Product.Create(
            "iPhone 13",
            Slug.Create("iphone-13").Value,
            Money.FromDollars(999).Value,
            "IP13-64GB");
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var newName = "iPhone 13 Pro";
        var newSlugResult = Slug.Create("iphone-13-pro");
        Assert.True(newSlugResult.IsSuccess);
        var newSlug = newSlugResult.Value;

        var newPriceResult = Money.FromDollars(1099);
        Assert.True(newPriceResult.IsSuccess);
        var newPrice = newPriceResult.Value;

        var newDescription = "Pro model with better camera";
        var newSku = "IP13P-128GB";

        // Act
        var result = product.Update(newName, newSlug, newPrice, newDescription, newSku);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newName, product.Name);
        Assert.Equal(newSlug, product.Slug);
        Assert.Equal(newPrice, product.BasePrice);
        Assert.Equal(newDescription, product.Description);
        Assert.Equal(newSku, product.Sku);
        Assert.Contains(product.DomainEvents, e => e is ProductUpdatedEvent);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateProduct()
    {
        // Arrange
        var productResult = Product.CreateInactive(
            "iPhone 13",
            Slug.Create("iphone-13").Value,
            Money.FromDollars(999).Value);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        Assert.False(product.IsActive);

        // Act
        var result = product.Activate();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(product.IsActive);
        Assert.Contains(product.DomainEvents, e => e is ProductStatusChangedEvent);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateProduct()
    {
        // Arrange
        var productResult = Product.Create(
            "iPhone 13",
            Slug.Create("iphone-13").Value,
            Money.FromDollars(999).Value);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        Assert.True(product.IsActive);

        // Act
        var result = product.Deactivate();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(product.IsActive);
        Assert.Contains(product.DomainEvents, e => e is ProductStatusChangedEvent);
    }

    [Fact]
    public void AddCategory_ShouldAddCategoryToProduct()
    {
        // Arrange
        var productResult = Product.Create(
            "iPhone 13",
            Slug.Create("iphone-13").Value,
            Money.FromDollars(999).Value);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var categoryResult = Category.Create(
            "Smartphones",
            Slug.Create("smartphones").Value);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;

        // Act
        var result = product.AddCategory(category);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(product.Categories);
        Assert.Equal(category.Id, product.Categories.First().CategoryId);
        Assert.Contains(product.DomainEvents, e => e is ProductCategoryAddedEvent);
    }

    [Fact]
    public void AddCategory_WhenAlreadyAdded_ShouldNotAddAgain()
    {
        // Arrange
        var productResult = Product.Create(
            "iPhone 13",
            Slug.Create("iphone-13").Value,
            Money.FromDollars(999).Value);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var categoryResult = Category.Create(
            "Smartphones",
            Slug.Create("smartphones").Value);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;

        product.AddCategory(category);

        // Pre-check
        Assert.Single(product.Categories);

        // Act
        var result = product.AddCategory(category);

        // Assert - still only one category
        Assert.True(result.IsSuccess);
        Assert.Single(product.Categories);
    }

    [Fact]
    public void AddCategory_WithNullCategory_ShouldReturnFailure()
    {
        // Arrange
        var productResult = Product.Create(
            "iPhone 13",
            Slug.Create("iphone-13").Value,
            Money.FromDollars(999).Value);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        // Act
        var result = product.AddCategory(null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Category.NotFound", result.Error.Code);
    }

    [Fact]
    public void RemoveCategory_ShouldRemoveCategoryFromProduct()
    {
        // Arrange
        var productResult = Product.Create(
            "iPhone 13",
            Slug.Create("iphone-13").Value,
            Money.FromDollars(999).Value);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var categoryResult = Category.Create(
            "Smartphones",
            Slug.Create("smartphones").Value);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;

        product.AddCategory(category);
        Assert.Single(product.Categories);

        // Act
        var result = product.RemoveCategory(category);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(product.Categories);
        Assert.Contains(product.DomainEvents, e => e is ProductCategoryRemovedEvent);
    }

    [Fact]
    public void AddAttribute_ShouldAddAttributeToProduct()
    {
        // Arrange
        var productResult = Product.Create(
            "iPhone 13",
            Slug.Create("iphone-13").Value,
            Money.FromDollars(999).Value);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var attributeResult = Attribute.Create("Color", "Color", AttributeType.Color);
        Assert.True(attributeResult.IsSuccess);
        var attribute = attributeResult.Value;

        var attributeValue = "Blue";

        // Act
        var result = product.AddAttribute(attribute, attributeValue);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(product.Attributes);
        Assert.Equal(attribute.Id, product.Attributes.First().AttributeId);
    }

    [Fact]
    public void AddVariant_ShouldAddVariantToProduct()
    {
        // Arrange
        var productResult = Product.Create(
            "iPhone 13",
            Slug.Create("iphone-13").Value,
            Money.FromDollars(999).Value);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var variantResult = ProductVariant.Create(
            product,
            "IP13-128GB",
            Money.FromDollars(1099).Value,
            100);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        // Act
        var result = product.AddVariant(variant);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(product.Variants);
        Assert.Equal(variant.Id, product.Variants.First().Id);
        Assert.Contains(product.DomainEvents, e => e is ProductVariantAddedEvent);
    }

    [Fact]
    public void AddVariant_WithDuplicateSku_ShouldReturnFailure()
    {
        // Arrange
        var productResult = Product.Create(
            "iPhone 13",
            Slug.Create("iphone-13").Value,
            Money.FromDollars(999).Value);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var sku = "IP13-128GB";
        var variant1Result = ProductVariant.Create(product, sku, Money.FromDollars(1099).Value, 100);
        Assert.True(variant1Result.IsSuccess);
        var variant1 = variant1Result.Value;

        product.AddVariant(variant1);

        var variant2Result = ProductVariant.Create(product, sku, Money.FromDollars(1199).Value, 50);
        Assert.True(variant2Result.IsSuccess);
        var variant2 = variant2Result.Value;

        // Act
        var result = product.AddVariant(variant2);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ProductVariant.DuplicateSku", result.Error.Code);
    }

    [Fact]
    public void UpdateMetadata_ShouldUpdateProductMetadata()
    {
        // Arrange
        var productResult = Product.Create(
            "iPhone 13",
            Slug.Create("iphone-13").Value,
            Money.FromDollars(999).Value);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var key = "weight";
        var value = "174g";

        // Act
        var result = product.UpdateMetadata(key, value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(product.Metadata.ContainsKey(key));
        Assert.Equal(value, product.Metadata[key]);
    }
}