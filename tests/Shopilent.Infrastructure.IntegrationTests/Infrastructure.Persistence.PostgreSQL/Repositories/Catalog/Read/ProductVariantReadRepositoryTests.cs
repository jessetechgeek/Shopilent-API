using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Infrastructure.IntegrationTests.Common;
using Shopilent.Infrastructure.IntegrationTests.TestData.Builders;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Persistence.PostgreSQL.Repositories.Catalog.Read;

[Collection("IntegrationTests")]
public class ProductVariantReadRepositoryTests : IntegrationTestBase
{
    private IUnitOfWork _unitOfWork = null!;

    public ProductVariantReadRepositoryTests(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _unitOfWork = GetService<IUnitOfWork>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProductVariant_ShouldReturnProductVariantDto()
    {
        // Arrange
        await ResetDatabaseAsync();
        
        // Create product first
        var product = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Create variant
        var variant = ProductVariantBuilder.Random().BuildForProduct(product);
        await _unitOfWork.ProductVariantWriter.AddAsync(variant);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductVariantReader.GetByIdAsync(variant.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(variant.Id);
        result.ProductId.Should().Be(product.Id);
        result.Sku.Should().Be(variant.Sku);
        result.StockQuantity.Should().Be(variant.StockQuantity);
        result.IsActive.Should().Be(variant.IsActive);
        if (variant.Price != null)
        {
            result.Price.Should().Be(variant.Price.Amount);
            result.Currency.Should().Be(variant.Price.Currency);
        }
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentProductVariant_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _unitOfWork.ProductVariantReader.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySkuAsync_ExistingSku_ShouldReturnProductVariantDto()
    {
        // Arrange
        await ResetDatabaseAsync();
        
        var product = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var uniqueSku = $"TEST-SKU-{DateTime.Now.Ticks}";
        var variant = ProductVariantBuilder.Random()
            .WithSku(uniqueSku)
            .BuildForProduct(product);
        await _unitOfWork.ProductVariantWriter.AddAsync(variant);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductVariantReader.GetBySkuAsync(uniqueSku);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(variant.Id);
        result.Sku.Should().Be(uniqueSku);
        result.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public async Task GetBySkuAsync_NonExistentSku_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentSku = "NON-EXISTENT-SKU";

        // Act
        var result = await _unitOfWork.ProductVariantReader.GetBySkuAsync(nonExistentSku);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SkuExistsAsync_ExistingSku_ShouldReturnTrue()
    {
        // Arrange
        await ResetDatabaseAsync();
        
        var product = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var existingSku = $"EXISTING-SKU-{DateTime.Now.Ticks}";
        var variant = ProductVariantBuilder.Random()
            .WithSku(existingSku)
            .BuildForProduct(product);
        await _unitOfWork.ProductVariantWriter.AddAsync(variant);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductVariantReader.SkuExistsAsync(existingSku);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SkuExistsAsync_NonExistentSku_ShouldReturnFalse()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentSku = $"NON-EXISTENT-{DateTime.Now.Ticks}";

        // Act
        var result = await _unitOfWork.ProductVariantReader.SkuExistsAsync(nonExistentSku);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SkuExistsAsync_ExistingSkuWithExcludeId_ShouldReturnFalse()
    {
        // Arrange
        await ResetDatabaseAsync();
        
        var product = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var sku = $"EXCLUDE-TEST-{DateTime.Now.Ticks}";
        var variant = ProductVariantBuilder.Random()
            .WithSku(sku)
            .BuildForProduct(product);
        await _unitOfWork.ProductVariantWriter.AddAsync(variant);
        await _unitOfWork.SaveChangesAsync();

        // Act - Exclude the current variant ID
        var result = await _unitOfWork.ProductVariantReader.SkuExistsAsync(sku, variant.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByProductIdAsync_ProductWithVariants_ShouldReturnAllVariants()
    {
        // Arrange
        await ResetDatabaseAsync();
        
        var product = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var variants = ProductVariantBuilder.CreateManyForProduct(product, 3);
        foreach (var variant in variants)
        {
            await _unitOfWork.ProductVariantWriter.AddAsync(variant);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductVariantReader.GetByProductIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.All(v => v.ProductId == product.Id).Should().BeTrue();
        result.Select(v => v.Id).Should().BeEquivalentTo(variants.Select(v => v.Id));
    }

    [Fact]
    public async Task GetByProductIdAsync_ProductWithNoVariants_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();
        
        var product = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductVariantReader.GetByProductIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByProductIdAsync_NonExistentProduct_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentProductId = Guid.NewGuid();

        // Act
        var result = await _unitOfWork.ProductVariantReader.GetByProductIdAsync(nonExistentProductId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetInStockVariantsAsync_ProductWithMixedStockVariants_ShouldReturnOnlyInStockVariants()
    {
        // Arrange
        await ResetDatabaseAsync();
        
        var product = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var inStockVariant1 = ProductVariantBuilder.InStock(10).BuildForProduct(product);
        var inStockVariant2 = ProductVariantBuilder.InStock(5).BuildForProduct(product);
        var outOfStockVariant = ProductVariantBuilder.OutOfStockVariant().BuildForProduct(product);

        await _unitOfWork.ProductVariantWriter.AddAsync(inStockVariant1);
        await _unitOfWork.ProductVariantWriter.AddAsync(inStockVariant2);
        await _unitOfWork.ProductVariantWriter.AddAsync(outOfStockVariant);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductVariantReader.GetInStockVariantsAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(v => v.Id == inStockVariant1.Id && v.StockQuantity > 0);
        result.Should().Contain(v => v.Id == inStockVariant2.Id && v.StockQuantity > 0);
        result.Should().NotContain(v => v.Id == outOfStockVariant.Id);
    }

    [Fact]
    public async Task GetInStockVariantsAsync_ProductWithNoInStockVariants_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();
        
        var product = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var outOfStockVariant1 = ProductVariantBuilder.OutOfStockVariant().BuildForProduct(product);
        var outOfStockVariant2 = ProductVariantBuilder.OutOfStockVariant().BuildForProduct(product);

        await _unitOfWork.ProductVariantWriter.AddAsync(outOfStockVariant1);
        await _unitOfWork.ProductVariantWriter.AddAsync(outOfStockVariant2);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductVariantReader.GetInStockVariantsAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }


    [Fact]
    public async Task ListAllAsync_HasProductVariants_ShouldReturnAllVariants()
    {
        // Arrange
        await ResetDatabaseAsync();
        
        var product1 = ProductBuilder.Random().Build();
        var product2 = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product1);
        await _unitOfWork.ProductWriter.AddAsync(product2);
        await _unitOfWork.SaveChangesAsync();

        var variants1 = ProductVariantBuilder.CreateManyForProduct(product1, 2);
        var variants2 = ProductVariantBuilder.CreateManyForProduct(product2, 3);
        
        foreach (var variant in variants1.Concat(variants2))
        {
            await _unitOfWork.ProductVariantWriter.AddAsync(variant);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductVariantReader.ListAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.Should().Contain(v => variants1.Any(v1 => v1.Id == v.Id));
        result.Should().Contain(v => variants2.Any(v2 => v2.Id == v.Id));
    }

    [Fact]
    public async Task ActiveAndInactiveVariants_ShouldHaveCorrectStatus()
    {
        // Arrange
        await ResetDatabaseAsync();
        
        var product = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var activeVariant = ProductVariantBuilder.Random().AsActive().BuildForProduct(product);
        var inactiveVariant = ProductVariantBuilder.InactiveVariant().BuildForProduct(product);

        await _unitOfWork.ProductVariantWriter.AddAsync(activeVariant);
        await _unitOfWork.ProductVariantWriter.AddAsync(inactiveVariant);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductVariantReader.ListAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(v => v.Id == activeVariant.Id && v.IsActive);
        result.Should().Contain(v => v.Id == inactiveVariant.Id && !v.IsActive);
    }

    [Fact]
    public async Task VariantsWithDifferentStockLevels_ShouldReturnCorrectStockQuantities()
    {
        // Arrange
        await ResetDatabaseAsync();
        
        var product = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var highStockVariant = ProductVariantBuilder.InStock(100).BuildForProduct(product);
        var lowStockVariant = ProductVariantBuilder.InStock(1).BuildForProduct(product);
        var outOfStockVariant = ProductVariantBuilder.OutOfStockVariant().BuildForProduct(product);

        await _unitOfWork.ProductVariantWriter.AddAsync(highStockVariant);
        await _unitOfWork.ProductVariantWriter.AddAsync(lowStockVariant);
        await _unitOfWork.ProductVariantWriter.AddAsync(outOfStockVariant);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductVariantReader.GetByProductIdAsync(product.Id);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(v => v.Id == highStockVariant.Id && v.StockQuantity == 100);
        result.Should().Contain(v => v.Id == lowStockVariant.Id && v.StockQuantity == 1);
        result.Should().Contain(v => v.Id == outOfStockVariant.Id && v.StockQuantity == 0);
    }
}