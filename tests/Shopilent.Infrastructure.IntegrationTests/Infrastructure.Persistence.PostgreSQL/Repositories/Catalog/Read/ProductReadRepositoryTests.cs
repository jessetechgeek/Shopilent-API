using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Common.Models;
using Shopilent.Infrastructure.IntegrationTests.Common;
using Shopilent.Infrastructure.IntegrationTests.TestData.Builders;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Persistence.PostgreSQL.Repositories.Catalog.Read;

[Collection("IntegrationTests")]
public class ProductReadRepositoryTests : IntegrationTestBase
{
    private IUnitOfWork _unitOfWork = null!;

    public ProductReadRepositoryTests(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _unitOfWork = GetService<IUnitOfWork>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ShouldReturnProductDto()
    {
        // Arrange
        await ResetDatabaseAsync();
        var product = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductReader.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
        result.Description.Should().Be(product.Description);
        result.Slug.Should().Be(product.Slug.Value);
        result.IsActive.Should().Be(product.IsActive);
        result.BasePrice.Should().Be(product.BasePrice.Amount);
        result.Currency.Should().Be(product.BasePrice.Currency);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentProduct_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _unitOfWork.ProductReader.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDetailByIdAsync_ExistingProduct_ShouldReturnProductDetailDto()
    {
        // Arrange
        await ResetDatabaseAsync();
        var product = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductReader.GetDetailByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
        result.Description.Should().Be(product.Description);
        result.Slug.Should().Be(product.Slug.Value);
        result.IsActive.Should().Be(product.IsActive);
        result.BasePrice.Should().Be(product.BasePrice.Amount);
        result.Currency.Should().Be(product.BasePrice.Currency);
        result.CreatedAt.Should().BeCloseTo(product.CreatedAt, TimeSpan.FromMilliseconds(100));
        result.UpdatedAt.Should().BeCloseTo(product.UpdatedAt, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task GetDetailByIdAsync_NonExistentProduct_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _unitOfWork.ProductReader.GetDetailByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_ExistingSlug_ShouldReturnProductDto()
    {
        // Arrange
        await ResetDatabaseAsync();
        var product = ProductBuilder.Random()
            .WithSlug($"test-product-{DateTime.Now.Ticks}")
            .Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductReader.GetBySlugAsync(product.Slug.Value);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Slug.Should().Be(product.Slug.Value);
        result.Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task GetBySlugAsync_NonExistentSlug_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentSlug = "non-existent-product";

        // Act
        var result = await _unitOfWork.ProductReader.GetBySlugAsync(nonExistentSlug);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SlugExistsAsync_ExistingSlug_ShouldReturnTrue()
    {
        // Arrange
        await ResetDatabaseAsync();
        var product = ProductBuilder.Random()
            .WithSlug($"existing-product-{DateTime.Now.Ticks}")
            .Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductReader.SlugExistsAsync(product.Slug.Value);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SlugExistsAsync_NonExistentSlug_ShouldReturnFalse()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentSlug = $"non-existent-{DateTime.Now.Ticks}";

        // Act
        var result = await _unitOfWork.ProductReader.SlugExistsAsync(nonExistentSlug);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SlugExistsAsync_ExistingSlugWithExcludeId_ShouldReturnFalse()
    {
        // Arrange
        await ResetDatabaseAsync();
        var product = ProductBuilder.Random()
            .WithSlug($"exclude-test-{DateTime.Now.Ticks}")
            .Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Act - Exclude the current product ID
        var result = await _unitOfWork.ProductReader.SlugExistsAsync(product.Slug.Value, product.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SkuExistsAsync_ExistingSku_ShouldReturnTrue()
    {
        // Arrange
        await ResetDatabaseAsync();
        var sku = $"SKU-{DateTime.Now.Ticks}";
        var product = ProductBuilder.Random().Build();
        // Update with SKU - need to access internal method or create via domain method
        product.Update(product.Name, product.Slug, product.BasePrice, product.Description, sku);
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductReader.SkuExistsAsync(sku);

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
        var result = await _unitOfWork.ProductReader.SkuExistsAsync(nonExistentSku);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdsAsync_ExistingIds_ShouldReturnProducts()
    {
        // Arrange
        await ResetDatabaseAsync();
        var products = ProductBuilder.CreateMany(3);
        foreach (var product in products)
        {
            await _unitOfWork.ProductWriter.AddAsync(product);
        }
        await _unitOfWork.SaveChangesAsync();

        var productIds = products.Select(p => p.Id).ToList();

        // Act
        var result = await _unitOfWork.ProductReader.GetByIdsAsync(productIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Select(p => p.Id).Should().BeEquivalentTo(productIds);
    }

    [Fact]
    public async Task GetByIdsAsync_MixedExistingAndNonExistentIds_ShouldReturnOnlyExistingProducts()
    {
        // Arrange
        await ResetDatabaseAsync();
        var product = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var ids = new List<Guid> { product.Id, Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var result = await _unitOfWork.ProductReader.GetByIdsAsync(ids);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task GetByCategoryAsync_ProductsInCategory_ShouldReturnProducts()
    {
        // Arrange
        await ResetDatabaseAsync();
        
        // Create category first
        var category = CategoryBuilder.Random().Build();
        await _unitOfWork.CategoryWriter.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        // Create products and assign to category
        var products = ProductBuilder.CreateMany(2);
        foreach (var product in products)
        {
            product.AddCategory(category);
            await _unitOfWork.ProductWriter.AddAsync(product);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductReader.GetByCategoryAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(p => products.Any(prod => prod.Id == p.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task GetByCategoryAsync_NoProductsInCategory_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();
        
        var category = CategoryBuilder.Random().Build();
        await _unitOfWork.CategoryWriter.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductReader.GetByCategoryAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_MatchingProductName_ShouldReturnProducts()
    {
        // Arrange
        await ResetDatabaseAsync();
        var searchTerm = "Unique";
        var product1 = ProductBuilder.Random()
            .WithName($"{searchTerm} Product 1")
            .Build();
        var product2 = ProductBuilder.Random()
            .WithName($"Another {searchTerm} Product")
            .Build();
        var product3 = ProductBuilder.Random()
            .WithName("Different Product")
            .Build();

        await _unitOfWork.ProductWriter.AddAsync(product1);
        await _unitOfWork.ProductWriter.AddAsync(product2);
        await _unitOfWork.ProductWriter.AddAsync(product3);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductReader.SearchAsync(searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == product1.Id);
        result.Should().Contain(p => p.Id == product2.Id);
        result.Should().NotContain(p => p.Id == product3.Id);
    }

    [Fact]
    public async Task SearchAsync_NoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();
        var product = ProductBuilder.Random().Build();
        await _unitOfWork.ProductWriter.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductReader.SearchAsync("NonExistentSearchTerm");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAllAsync_HasProducts_ShouldReturnAllProducts()
    {
        // Arrange
        await ResetDatabaseAsync();
        var products = ProductBuilder.CreateMany(5);
        foreach (var product in products)
        {
            await _unitOfWork.ProductWriter.AddAsync(product);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductReader.ListAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task ListAllAsync_ActiveAndInactiveProducts_ShouldReturnAll()
    {
        // Arrange
        await ResetDatabaseAsync();
        var activeProduct = ProductBuilder.Random().AsActive().Build();
        var inactiveProduct = ProductBuilder.Random().AsInactive().Build();

        await _unitOfWork.ProductWriter.AddAsync(activeProduct);
        await _unitOfWork.ProductWriter.AddAsync(inactiveProduct);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.ProductReader.ListAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == activeProduct.Id && p.IsActive);
        result.Should().Contain(p => p.Id == inactiveProduct.Id && !p.IsActive);
    }

    [Fact]
    public async Task GetProductDetailDataTableAsync_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        await ResetDatabaseAsync();
        var products = ProductBuilder.CreateMany(10);
        foreach (var product in products)
        {
            await _unitOfWork.ProductWriter.AddAsync(product);
        }
        await _unitOfWork.SaveChangesAsync();

        var request = new DataTableRequest
        {
            Start = 0,
            Length = 5,
            Search = new DataTableSearch { Value = "" },
            Order = new List<DataTableOrder>
            {
                new DataTableOrder { Column = 0, Dir = "asc" }
            },
            Columns = new List<DataTableColumn>
            {
                new DataTableColumn { Data = "name", Name = "name", Searchable = true, Orderable = true }
            }
        };

        // Act
        var result = await _unitOfWork.ProductReader.GetProductDetailDataTableAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.RecordsFiltered.Should().Be(10);
        result.RecordsTotal.Should().Be(10);
        result.Data.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetProductDetailDataTableAsync_WithSearch_ShouldReturnFilteredResults()
    {
        // Arrange
        await ResetDatabaseAsync();
        var searchableProduct = ProductBuilder.Random()
            .WithName("Searchable Product")
            .Build();
        var regularProduct = ProductBuilder.Random()
            .WithName("Regular Product")
            .Build();

        await _unitOfWork.ProductWriter.AddAsync(searchableProduct);
        await _unitOfWork.ProductWriter.AddAsync(regularProduct);
        await _unitOfWork.SaveChangesAsync();

        var request = new DataTableRequest
        {
            Start = 0,
            Length = 10,
            Search = new DataTableSearch { Value = "Searchable" },
            Order = new List<DataTableOrder>
            {
                new DataTableOrder { Column = 0, Dir = "asc" }
            },
            Columns = new List<DataTableColumn>
            {
                new DataTableColumn { Data = "name", Name = "name", Searchable = true, Orderable = true }
            }
        };

        // Act
        var result = await _unitOfWork.ProductReader.GetProductDetailDataTableAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.RecordsTotal.Should().Be(2); // Total before filtering
        result.RecordsFiltered.Should().Be(1); // After filtering
        result.Data.Should().HaveCount(1);
        result.Data.First().Name.Should().Be("Searchable Product");
    }
}