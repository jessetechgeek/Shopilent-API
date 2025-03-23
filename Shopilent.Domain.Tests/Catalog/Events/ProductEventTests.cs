using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Events;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Sales.ValueObjects;

namespace Shopilent.Domain.Tests.Catalog.Events;

public class ProductEventTests
{
    [Fact]
    public void Product_WhenCreated_ShouldRaiseProductCreatedEvent()
    {
        // Arrange & Act
        var slugResult = Slug.Create("test-product");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        var moneyResult = Money.FromDollars(100);
        Assert.True(moneyResult.IsSuccess);
        var money = moneyResult.Value;

        var productResult = Product.Create("Test Product", slug, money);

        // Assert
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;
        var domainEvent = Assert.Single(product.DomainEvents, e => e is ProductCreatedEvent);
        var createdEvent = (ProductCreatedEvent)domainEvent;
        Assert.Equal(product.Id, createdEvent.ProductId);
    }

    [Fact]
    public void Product_WhenUpdated_ShouldRaiseProductUpdatedEvent()
    {
        // Arrange
        var slugResult = Slug.Create("test-product");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        var moneyResult = Money.FromDollars(100);
        Assert.True(moneyResult.IsSuccess);
        var money = moneyResult.Value;

        var productResult = Product.Create("Test Product", slug, money);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        product.ClearDomainEvents(); // Clear the creation event

        var newSlugResult = Slug.Create("updated-product");
        Assert.True(newSlugResult.IsSuccess);
        var newSlug = newSlugResult.Value;

        var newPriceResult = Money.FromDollars(120);
        Assert.True(newPriceResult.IsSuccess);
        var newPrice = newPriceResult.Value;

        // Act
        var updateResult = product.Update(
            "Updated Product",
            newSlug,
            newPrice,
            "Updated description");
        Assert.True(updateResult.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(product.DomainEvents, e => e is ProductUpdatedEvent);
        var updatedEvent = (ProductUpdatedEvent)domainEvent;
        Assert.Equal(product.Id, updatedEvent.ProductId);
    }

    [Fact]
    public void Product_WhenActivated_ShouldRaiseProductStatusChangedEvent()
    {
        // Arrange
        var slugResult = Slug.Create("test-product");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        var moneyResult = Money.FromDollars(100);
        Assert.True(moneyResult.IsSuccess);
        var money = moneyResult.Value;

        var productResult = Product.CreateInactive("Test Product", slug, money);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        product.ClearDomainEvents(); // Clear the creation event

        // Act
        var result = product.Activate();
        Assert.True(result.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(product.DomainEvents, e => e is ProductStatusChangedEvent);
        var statusEvent = (ProductStatusChangedEvent)domainEvent;
        Assert.Equal(product.Id, statusEvent.ProductId);
        Assert.True(statusEvent.IsActive);
    }

    [Fact]
    public void Product_WhenDeactivated_ShouldRaiseProductStatusChangedEvent()
    {
        // Arrange
        var slugResult = Slug.Create("test-product");
        Assert.True(slugResult.IsSuccess);
        var slug = slugResult.Value;

        var moneyResult = Money.FromDollars(100);
        Assert.True(moneyResult.IsSuccess);
        var money = moneyResult.Value;

        var productResult = Product.Create("Test Product", slug, money);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        product.ClearDomainEvents(); // Clear the creation event

        // Act
        var result = product.Deactivate();
        Assert.True(result.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(product.DomainEvents, e => e is ProductStatusChangedEvent);
        var statusEvent = (ProductStatusChangedEvent)domainEvent;
        Assert.Equal(product.Id, statusEvent.ProductId);
        Assert.False(statusEvent.IsActive);
    }

    [Fact]
    public void Product_WhenCategoryAdded_ShouldRaiseProductCategoryAddedEvent()
    {
        // Arrange
        var productSlugResult = Slug.Create("test-product");
        Assert.True(productSlugResult.IsSuccess);
        var productSlug = productSlugResult.Value;

        var moneyResult = Money.FromDollars(100);
        Assert.True(moneyResult.IsSuccess);
        var money = moneyResult.Value;

        var productResult = Product.Create("Test Product", productSlug, money);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var categorySlugResult = Slug.Create("test-category");
        Assert.True(categorySlugResult.IsSuccess);
        var categorySlug = categorySlugResult.Value;

        var categoryResult = Category.Create("Test Category", categorySlug);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;

        product.ClearDomainEvents(); // Clear the creation event

        // Act
        var result = product.AddCategory(category);
        Assert.True(result.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(product.DomainEvents, e => e is ProductCategoryAddedEvent);
        var categoryEvent = (ProductCategoryAddedEvent)domainEvent;
        Assert.Equal(product.Id, categoryEvent.ProductId);
        Assert.Equal(category.Id, categoryEvent.CategoryId);
    }

    [Fact]
    public void Product_WhenCategoryRemoved_ShouldRaiseProductCategoryRemovedEvent()
    {
        // Arrange
        var productSlugResult = Slug.Create("test-product");
        Assert.True(productSlugResult.IsSuccess);
        var productSlug = productSlugResult.Value;

        var moneyResult = Money.FromDollars(100);
        Assert.True(moneyResult.IsSuccess);
        var money = moneyResult.Value;

        var productResult = Product.Create("Test Product", productSlug, money);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var categorySlugResult = Slug.Create("test-category");
        Assert.True(categorySlugResult.IsSuccess);
        var categorySlug = categorySlugResult.Value;

        var categoryResult = Category.Create("Test Category", categorySlug);
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;

        var addCategoryResult = product.AddCategory(category);
        Assert.True(addCategoryResult.IsSuccess);

        product.ClearDomainEvents(); // Clear previous events

        // Act
        var result = product.RemoveCategory(category);
        Assert.True(result.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(product.DomainEvents, e => e is ProductCategoryRemovedEvent);
        var categoryEvent = (ProductCategoryRemovedEvent)domainEvent;
        Assert.Equal(product.Id, categoryEvent.ProductId);
        Assert.Equal(category.Id, categoryEvent.CategoryId);
    }

    [Fact]
    public void Product_WhenVariantAdded_ShouldRaiseProductVariantAddedEvent()
    {
        // Arrange
        var productSlugResult = Slug.Create("test-product");
        Assert.True(productSlugResult.IsSuccess);
        var productSlug = productSlugResult.Value;

        var moneyResult = Money.FromDollars(100);
        Assert.True(moneyResult.IsSuccess);
        var money = moneyResult.Value;

        var productResult = Product.Create("Test Product", productSlug, money);
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var variantPriceResult = Money.FromDollars(120);
        Assert.True(variantPriceResult.IsSuccess);
        var variantPrice = variantPriceResult.Value;

        var variantResult = ProductVariant.Create(product, "V1", variantPrice, 10);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        product.ClearDomainEvents(); // Clear the creation event

        // Act
        var result = product.AddVariant(variant);
        Assert.True(result.IsSuccess);

        // Assert
        var domainEvent = Assert.Single(product.DomainEvents, e => e is ProductVariantAddedEvent);
        var variantEvent = (ProductVariantAddedEvent)domainEvent;
        Assert.Equal(product.Id, variantEvent.ProductId);
        Assert.Equal(variant.Id, variantEvent.VariantId);
    }
}