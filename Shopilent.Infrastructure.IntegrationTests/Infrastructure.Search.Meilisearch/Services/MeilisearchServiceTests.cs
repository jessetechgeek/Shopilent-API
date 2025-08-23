using Shopilent.Application.Abstractions.Search;
using Shopilent.Infrastructure.IntegrationTests.Common;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Search.Meilisearch.Services;

[Collection("IntegrationTests")]
public class MeilisearchServiceTests : IntegrationTestBase
{
    private ISearchService _searchService = null!;

    public MeilisearchServiceTests(IntegrationTestFixture integrationTestFixture) : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _searchService = GetService<ISearchService>();
        return Task.CompletedTask;
    }

    private static ProductSearchDocument CreateTestProduct(
        Guid? id = null,
        string name = "Test Product",
        string description = "Test description",
        string sku = "TEST-001",
        string slug = "test-product",
        decimal basePrice = 99.99m,
        bool isActive = true,
        string categoryName = "Test Category",
        string categorySlug = "test-category",
        int totalStock = 10,
        bool hasStock = true)
    {
        return new ProductSearchDocument
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            SKU = sku,
            Slug = slug,
            BasePrice = basePrice,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Categories = new[]
            {
                new ProductSearchCategory 
                { 
                    Id = Guid.NewGuid(), 
                    Name = categoryName, 
                    Slug = categorySlug,
                    HierarchyPath = categoryName
                }
            },
            Attributes = Array.Empty<ProductSearchAttribute>(),
            Images = Array.Empty<ProductSearchImage>(),
            Variants = Array.Empty<ProductSearchVariant>(),
            TotalStock = totalStock,
            HasStock = hasStock,
            Status = "Active",
            CategorySlugs = new[] { categorySlug },
            VariantSKUs = Array.Empty<string>(),
            PriceRange = new ProductSearchPriceRange { Min = basePrice, Max = basePrice },
            AttributeFilters = new Dictionary<string, string[]>(),
            FlatAttributes = new Dictionary<string, string[]>()
        };
    }

    [Fact]
    public async Task IsHealthyAsync_WithRunningMeilisearch_ShouldReturnTrue()
    {
        // Act
        var isHealthy = await _searchService.IsHealthyAsync();

        // Assert
        isHealthy.Should().BeTrue();
    }

    [Fact]
    public async Task InitializeIndexesAsync_ShouldConfigureIndexSuccessfully()
    {
        // Act
        await _searchService.InitializeIndexesAsync();

        // Assert
        // If no exception is thrown, initialization was successful
        var isHealthy = await _searchService.IsHealthyAsync();
        isHealthy.Should().BeTrue();
    }

    [Fact]
    public async Task IndexProductAsync_WithValidDocument_ShouldReturnSuccess()
    {
        // Arrange
        await _searchService.InitializeIndexesAsync();
        
        var document = CreateTestProduct(
            name: "Test Product",
            description: "Test product description",
            sku: "TEST-001",
            slug: "test-product",
            basePrice: 99.99m,
            categoryName: "Electronics",
            categorySlug: "electronics"
        );

        // Act
        var result = await _searchService.IndexProductAsync(document);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task IndexProductsAsync_WithValidDocuments_ShouldReturnSuccess()
    {
        // Arrange
        await _searchService.InitializeIndexesAsync();
        
        var documents = new[]
        {
            CreateTestProduct(
                name: "Product 1",
                description: "First test product",
                sku: "TEST-001",
                slug: "product-1",
                basePrice: 99.99m,
                categoryName: "Electronics",
                categorySlug: "electronics",
                totalStock: 10
            ),
            CreateTestProduct(
                name: "Product 2",
                description: "Second test product",
                sku: "TEST-002",
                slug: "product-2",
                basePrice: 149.99m,
                categoryName: "Books",
                categorySlug: "books",
                totalStock: 5
            )
        };

        // Act
        var result = await _searchService.IndexProductsAsync(documents);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task IndexProductsAsync_WithEmptyCollection_ShouldReturnSuccess()
    {
        // Arrange
        await _searchService.InitializeIndexesAsync();
        var emptyDocuments = Array.Empty<ProductSearchDocument>();

        // Act
        var result = await _searchService.IndexProductsAsync(emptyDocuments);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProductAsync_WithExistingProduct_ShouldReturnSuccess()
    {
        // Arrange
        await _searchService.InitializeIndexesAsync();
        
        var productId = Guid.NewGuid();
        var document = CreateTestProduct(
            id: productId,
            name: "Product to Delete",
            description: "This product will be deleted",
            sku: "DELETE-001",
            slug: "product-to-delete",
            basePrice: 79.99m,
            categoryName: "Test Category",
            categorySlug: "test-category",
            totalStock: 1
        );

        await _searchService.IndexProductAsync(document);

        // Act
        var result = await _searchService.DeleteProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProductAsync_WithNonExistentProduct_ShouldReturnSuccess()
    {
        // Arrange
        await _searchService.InitializeIndexesAsync();
        var nonExistentProductId = Guid.NewGuid();

        // Act
        var result = await _searchService.DeleteProductAsync(nonExistentProductId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SearchProductsAsync_WithBasicQuery_ShouldReturnResults()
    {
        // Arrange
        await _searchService.InitializeIndexesAsync();
        
        var productId = Guid.NewGuid();
        var document = CreateTestProduct(
            id: productId,
            name: "Searchable Product",
            description: "A product that can be found through search",
            sku: "SEARCH-001",
            slug: "searchable-product",
            basePrice: 199.99m,
            categoryName: "Electronics",
            categorySlug: "electronics",
            totalStock: 15
        );

        await _searchService.IndexProductAsync(document);

        // Wait a bit for indexing to complete
        await Task.Delay(1000);

        var searchRequest = new SearchRequest
        {
            Query = "Searchable",
            PageNumber = 1,
            PageSize = 10,
            ActiveOnly = true,
            CategorySlugs = Array.Empty<string>(),
            AttributeFilters = new Dictionary<string, string[]>(),
            SortBy = "relevance",
            SortDescending = false,
            InStockOnly = false
        };

        // Act
        var result = await _searchService.SearchProductsAsync(searchRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().NotBeEmpty();
        result.Value.Items.Should().Contain(item => item.Id == productId);
    }

    [Fact]
    public async Task SearchProductsAsync_WithEmptyQuery_ShouldReturnAllProducts()
    {
        // Arrange
        await _searchService.InitializeIndexesAsync();
        
        var documents = new[]
        {
            CreateTestProduct(
                name: "Product A",
                description: "First product",
                sku: "A-001",
                slug: "product-a",
                basePrice: 50.00m,
                categoryName: "Category A",
                categorySlug: "category-a",
                totalStock: 10
            ),
            CreateTestProduct(
                name: "Product B",
                description: "Second product",
                sku: "B-001",
                slug: "product-b",
                basePrice: 75.00m,
                categoryName: "Category B",
                categorySlug: "category-b",
                totalStock: 5
            )
        };

        await _searchService.IndexProductsAsync(documents);

        // Wait a bit for indexing to complete
        await Task.Delay(1000);

        var searchRequest = new SearchRequest
        {
            Query = "",
            PageNumber = 1,
            PageSize = 10,
            ActiveOnly = true,
            CategorySlugs = Array.Empty<string>(),
            AttributeFilters = new Dictionary<string, string[]>(),
            SortBy = "relevance",
            SortDescending = false,
            InStockOnly = false
        };

        // Act
        var result = await _searchService.SearchProductsAsync(searchRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Length.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task SearchProductsAsync_WithCategoryFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await _searchService.InitializeIndexesAsync();
        
        var electronicsProduct = CreateTestProduct(
            name: "Electronics Product",
            description: "An electronic device",
            sku: "ELEC-001",
            slug: "electronics-product",
            basePrice: 299.99m,
            categoryName: "Electronics",
            categorySlug: "electronics",
            totalStock: 8
        );

        var bookProduct = CreateTestProduct(
            name: "Book Product",
            description: "A great book",
            sku: "BOOK-001",
            slug: "book-product",
            basePrice: 19.99m,
            categoryName: "Books",
            categorySlug: "books",
            totalStock: 20
        );

        await _searchService.IndexProductsAsync(new[] { electronicsProduct, bookProduct });

        // Wait a bit for indexing to complete
        await Task.Delay(1000);

        var searchRequest = new SearchRequest
        {
            Query = "",
            PageNumber = 1,
            PageSize = 10,
            ActiveOnly = true,
            CategorySlugs = new[] { "electronics" },
            AttributeFilters = new Dictionary<string, string[]>(),
            SortBy = "relevance",
            SortDescending = false,
            InStockOnly = false
        };

        // Act
        var result = await _searchService.SearchProductsAsync(searchRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().NotBeEmpty();
        result.Value.Items.Should().Contain(item => item.Id == electronicsProduct.Id);
        result.Value.Items.Should().NotContain(item => item.Id == bookProduct.Id);
    }

    [Fact]
    public async Task SearchProductsAsync_WithPriceFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await _searchService.InitializeIndexesAsync();
        
        var expensiveProduct = CreateTestProduct(
            name: "Expensive Product",
            description: "A costly item",
            sku: "EXP-001",
            slug: "expensive-product",
            basePrice: 500.00m,
            categoryName: "Luxury",
            categorySlug: "luxury",
            totalStock: 3
        );

        var cheapProduct = CreateTestProduct(
            name: "Cheap Product",
            description: "An affordable item",
            sku: "CHEAP-001",
            slug: "cheap-product",
            basePrice: 25.00m,
            categoryName: "Budget",
            categorySlug: "budget",
            totalStock: 50
        );

        await _searchService.IndexProductsAsync(new[] { expensiveProduct, cheapProduct });

        // Wait a bit for indexing to complete
        await Task.Delay(1000);

        var searchRequest = new SearchRequest
        {
            Query = "",
            PageNumber = 1,
            PageSize = 10,
            ActiveOnly = true,
            CategorySlugs = Array.Empty<string>(),
            AttributeFilters = new Dictionary<string, string[]>(),
            PriceMin = 100.00m,
            PriceMax = 1000.00m,
            SortBy = "relevance",
            SortDescending = false,
            InStockOnly = false
        };

        // Act
        var result = await _searchService.SearchProductsAsync(searchRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().NotBeEmpty();
        result.Value.Items.Should().Contain(item => item.Id == expensiveProduct.Id);
        result.Value.Items.Should().NotContain(item => item.Id == cheapProduct.Id);
    }

    [Fact]
    public async Task SearchProductsAsync_WithInStockFilter_ShouldReturnOnlyInStockProducts()
    {
        // Arrange
        await _searchService.InitializeIndexesAsync();
        
        var inStockProduct = CreateTestProduct(
            name: "In Stock Product",
            description: "Available product",
            sku: "STOCK-001",
            slug: "in-stock-product",
            basePrice: 100.00m,
            categoryName: "Available",
            categorySlug: "available",
            totalStock: 10,
            hasStock: true
        );

        var outOfStockProduct = CreateTestProduct(
            name: "Out of Stock Product",
            description: "Unavailable product",
            sku: "NOSTOCK-001",
            slug: "out-of-stock-product",
            basePrice: 100.00m,
            categoryName: "Unavailable",
            categorySlug: "unavailable",
            totalStock: 0,
            hasStock: false
        );

        await _searchService.IndexProductsAsync(new[] { inStockProduct, outOfStockProduct });

        // Wait a bit for indexing to complete
        await Task.Delay(1000);

        var searchRequest = new SearchRequest
        {
            Query = "",
            PageNumber = 1,
            PageSize = 10,
            ActiveOnly = true,
            CategorySlugs = Array.Empty<string>(),
            AttributeFilters = new Dictionary<string, string[]>(),
            SortBy = "relevance",
            SortDescending = false,
            InStockOnly = true
        };

        // Act
        var result = await _searchService.SearchProductsAsync(searchRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().NotBeEmpty();
        result.Value.Items.Should().Contain(item => item.Id == inStockProduct.Id);
        result.Value.Items.Should().NotContain(item => item.Id == outOfStockProduct.Id);
    }
}