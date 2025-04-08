using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Enums;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Sales.ValueObjects;
using Attribute = Shopilent.Domain.Catalog.Attribute;

namespace Shopilent.Domain.Tests.Catalog;

public class ProductVariantTests
{
    private Product CreateTestProduct()
    {
        return Product.Create(
            "Test Product",
            Slug.Create("test-product").Value,
            Money.FromDollars(100).Value).Value;
    }

    private Attribute CreateTestAttribute(string name = "Color", AttributeType type = AttributeType.Color)
    {
        return Attribute.CreateVariant(name, name, type).Value;
    }

    [Fact]
    public void Create_WithValidParameters_ShouldCreateProductVariant()
    {
        // Arrange
        var product = CreateTestProduct();
        var sku = "TEST-123";
        var price = Money.FromDollars(150).Value;
        var stockQuantity = 100;

        // Act
        var result = ProductVariant.Create(product.Id, sku, price, stockQuantity);

        // Assert
        Assert.True(result.IsSuccess);
        var variant = result.Value;
        Assert.Equal(product.Id, variant.ProductId);
        Assert.Equal(sku, variant.Sku);
        Assert.Equal(price, variant.Price);
        Assert.Equal(stockQuantity, variant.StockQuantity);
        Assert.True(variant.IsActive);
        Assert.Empty(variant.Attributes);
        Assert.Empty(variant.Metadata);
    }

    [Fact]
    public void Create_WithInvalidProductId_ShouldReturnFailure()
    {
        // Arrange
        var productId = Guid.Empty;
        var sku = "TEST-123";
        var price = Money.FromDollars(150).Value;
        var stockQuantity = 100;

        // Act
        var result = ProductVariant.Create(productId, sku, price, stockQuantity);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Product.NotFound", result.Error.Code);
    }

    [Fact]
    public void Create_WithNegativeStockQuantity_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateTestProduct();
        var sku = "TEST-123";
        var price = Money.FromDollars(150).Value;
        var stockQuantity = -10;

        // Act
        var result = ProductVariant.Create(product.Id, sku, price, stockQuantity);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ProductVariant.NegativeStockQuantity", result.Error.Code);
    }

    [Fact]
    public void CreateInactive_ShouldCreateInactiveVariant()
    {
        // Arrange
        var product = CreateTestProduct();
        var sku = "TEST-123";
        var price = Money.FromDollars(150).Value;
        var stockQuantity = 100;

        // Act
        var result = ProductVariant.CreateInactive(product, sku, price, stockQuantity);

        // Assert
        Assert.True(result.IsSuccess);
        var variant = result.Value;
        Assert.Equal(product.Id, variant.ProductId);
        Assert.Equal(sku, variant.Sku);
        Assert.Equal(price, variant.Price);
        Assert.Equal(stockQuantity, variant.StockQuantity);
        Assert.False(variant.IsActive);
    }

    [Fact]
    public void CreateOutOfStock_ShouldCreateVariantWithZeroStock()
    {
        // Arrange
        var product = CreateTestProduct();
        var sku = "TEST-123";
        var price = Money.FromDollars(150).Value;

        // Act
        var result = ProductVariant.CreateOutOfStock(product, "TEST-123", price);


        // Assert
        Assert.True(result.IsSuccess);
        var variant = result.Value;
        Assert.Equal(product.Id, variant.ProductId);
        Assert.Equal(sku, variant.Sku);
        Assert.Equal(price, variant.Price);
        Assert.Equal(0, variant.StockQuantity);
        Assert.True(variant.IsActive);
    }

    [Fact]
    public void Update_ShouldUpdateSkuAndPrice()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "OLD-123", Money.FromDollars(100).Value, 10);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var newSku = "NEW-456";
        var newPrice = Money.FromDollars(150).Value;

        // Act
        var result = variant.Update(newSku, newPrice);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newSku, variant.Sku);
        Assert.Equal(newPrice, variant.Price);
    }

    [Fact]
    public void SetStockQuantity_WithValidQuantity_ShouldUpdateStock()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value, 10);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;
        Assert.Equal(10, variant.StockQuantity);

        var newQuantity = 50;

        // Act
        var result = variant.SetStockQuantity(newQuantity);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newQuantity, variant.StockQuantity);
    }

    [Fact]
    public void SetStockQuantity_WithNegativeQuantity_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value, 10);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;
        var negativeQuantity = -5;

        // Act
        var result = variant.SetStockQuantity(negativeQuantity);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ProductVariant.NegativeStockQuantity", result.Error.Code);
    }

    [Fact]
    public void AddStock_WithPositiveQuantity_ShouldIncreaseStock()
    {
        // Arrange
        var product = CreateTestProduct();
        var initialStock = 10;
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value, initialStock);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var additionalStock = 15;
        var expectedStock = initialStock + additionalStock;

        // Act
        var result = variant.AddStock(additionalStock);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedStock, variant.StockQuantity);
    }

    [Fact]
    public void AddStock_WithZeroOrNegativeQuantity_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value, 10);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        // Act & Assert - Zero
        var zeroResult = variant.AddStock(0);
        Assert.True(zeroResult.IsFailure);
        Assert.Equal("ProductVariant.NegativeStockQuantity", zeroResult.Error.Code);

        // Act & Assert - Negative
        var negativeResult = variant.AddStock(-5);
        Assert.True(negativeResult.IsFailure);
        Assert.Equal("ProductVariant.NegativeStockQuantity", negativeResult.Error.Code);
    }

    [Fact]
    public void RemoveStock_WithValidQuantity_ShouldDecreaseStock()
    {
        // Arrange
        var product = CreateTestProduct();
        var initialStock = 20;
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value, initialStock);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var quantityToRemove = 5;
        var expectedStock = initialStock - quantityToRemove;

        // Act
        var result = variant.RemoveStock(quantityToRemove);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedStock, variant.StockQuantity);
    }

    [Fact]
    public void RemoveStock_WithZeroOrNegativeQuantity_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value, 10);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        // Act & Assert - Zero
        var zeroResult = variant.RemoveStock(0);
        Assert.True(zeroResult.IsFailure);
        Assert.Equal("ProductVariant.NegativeStockQuantity", zeroResult.Error.Code);

        // Act & Assert - Negative
        var negativeResult = variant.RemoveStock(-5);
        Assert.True(negativeResult.IsFailure);
        Assert.Equal("ProductVariant.NegativeStockQuantity", negativeResult.Error.Code);
    }

    [Fact]
    public void RemoveStock_WithQuantityExceedingStock_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateTestProduct();
        var initialStock = 10;
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value, initialStock);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var quantityToRemove = initialStock + 5;

        // Act
        var result = variant.RemoveStock(quantityToRemove);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ProductVariant.InsufficientStock", result.Error.Code);
        Assert.Equal(initialStock, variant.StockQuantity); // Stock should remain unchanged
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateVariant()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.CreateInactive(product, "TEST-123", Money.FromDollars(100).Value);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;
        Assert.False(variant.IsActive);

        // Act
        var result = variant.Activate();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(variant.IsActive);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateVariant()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;
        Assert.True(variant.IsActive);

        // Act
        var result = variant.Deactivate();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(variant.IsActive);
    }

    [Fact]
    public void AddAttribute_ShouldAddAttributeToVariant()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var attribute = CreateTestAttribute();
        var attributeValue = "Blue";

        // Act
        var result = variant.AddAttribute(attribute, attributeValue);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(variant.Attributes);
        Assert.Equal(attribute.Id, variant.Attributes.First().AttributeId);
    }

    [Fact]
    public void AddAttribute_WithNonVariantAttribute_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var attributeResult = Attribute.Create("Weight", "Weight", AttributeType.Number);
        Assert.True(attributeResult.IsSuccess);
        var attribute = attributeResult.Value; // Not a variant attribute

        var attributeValue = 500;

        // Act
        var result = variant.AddAttribute(attribute, attributeValue);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ProductVariant.NonVariantAttribute", result.Error.Code);
    }

    [Fact]
    public void AddAttribute_WithNullAttribute_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        Attribute attribute = null;
        var attributeValue = "Blue";

        // Act
        var result = variant.AddAttribute(attribute, attributeValue);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Attribute.NotFound", result.Error.Code);
    }

    [Fact]
    public void UpdateMetadata_ShouldAddOrUpdateMetadata()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var key = "dimension";
        var value = "10x15x5 cm";

        // Act
        var result = variant.UpdateMetadata(key, value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(variant.Metadata.ContainsKey(key));
        Assert.Equal(value, variant.Metadata[key]);
    }

    [Fact]
    public void UpdateMetadata_WithEmptyKey_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var emptyKey = string.Empty;
        var value = "test";

        // Act
        var result = variant.UpdateMetadata(emptyKey, value);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ProductVariant.InvalidMetadataKey", result.Error.Code);
    }

    [Fact]
    public void HasAttribute_WithExistingAttributeId_ShouldReturnTrue()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var attribute = CreateTestAttribute();
        var attributeValue = "Blue";

        var addResult = variant.AddAttribute(attribute, attributeValue);
        Assert.True(addResult.IsSuccess);

        // Act
        var result = variant.HasAttribute(attribute.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAttribute_WithNonExistingAttributeId_ShouldReturnFalse()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var nonExistingAttributeId = Guid.NewGuid();

        // Act
        var result = variant.HasAttribute(nonExistingAttributeId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetAttributeValue_WithExistingAttribute_ShouldReturnValue()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var attribute = CreateTestAttribute();
        var attributeValue = "Blue";

        var addResult = variant.AddAttribute(attribute, attributeValue);
        Assert.True(addResult.IsSuccess);

        // Act
        var result = variant.GetAttributeValue(attribute.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(attributeValue, result.Value);
    }

    [Fact]
    public void GetAttributeValue_WithNonExistingAttribute_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var nonExistingAttributeId = Guid.NewGuid();

        // Act
        var result = variant.GetAttributeValue(nonExistingAttributeId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Attribute.NotFound", result.Error.Code);
    }

    [Fact]
    public void UpdateAttributeValue_WithExistingAttribute_ShouldUpdateValue()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var attribute = CreateTestAttribute();
        var initialValue = "Blue";
        var newValue = "Red";

        var addResult = variant.AddAttribute(attribute, initialValue);
        Assert.True(addResult.IsSuccess);

        // Act
        var updateResult = variant.UpdateAttributeValue(attribute.Id, newValue);

        // Assert
        Assert.True(updateResult.IsSuccess);

        var getValueResult = variant.GetAttributeValue(attribute.Id);
        Assert.True(getValueResult.IsSuccess);
        Assert.Equal(newValue, getValueResult.Value);
    }

    [Fact]
    public void UpdateAttributeValue_WithNonExistingAttribute_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateTestProduct();
        var variantResult = ProductVariant.Create(product.Id, "TEST-123", Money.FromDollars(100).Value);
        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var nonExistingAttributeId = Guid.NewGuid();
        var newValue = "Red";

        // Act
        var result = variant.UpdateAttributeValue(nonExistingAttributeId, newValue);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Attribute.NotFound", result.Error.Code);
    }
}