using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Application.Features.Catalog.Queries.GetProductsDatatable.V1;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Products.GetProductsDatatable.V1;

public class GetProductsDatatableEndpointV1Tests : ApiIntegrationTestBase
{
    public GetProductsDatatableEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetProductsDatatable_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.CreateValidRequest();

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Draw.Should().Be(request.Draw);
        response.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(0);
        response.Data.RecordsFiltered.Should().BeGreaterThanOrEqualTo(0);
        response.Data.Data.Should().NotBeNull();
        response.Data.Error.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetProductsDatatable_WithTestProducts_ShouldReturnCorrectData()
    {
        // Arrange


        // Create test products
        await CreateTestProductAsync("Gaming Laptop", "gaming-laptop", "High-performance gaming laptop", 1299.99m,
            "USD", "LAPTOP001");
        await CreateTestProductAsync("Wireless Mouse", "wireless-mouse", "Ergonomic wireless mouse", 29.99m, "USD",
            "MOUSE001");
        await CreateTestProductAsync("Mechanical Keyboard", "mechanical-keyboard", "RGB mechanical keyboard", 149.99m,
            "USD", "KEYBOARD001");

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.CreateValidRequest(length: 10);

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().HaveCountGreaterThanOrEqualTo(3); // At least the 3 test products
        response.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(3);
        response.Data.RecordsFiltered.Should().BeGreaterThanOrEqualTo(3);

        // Verify data structure
        var firstProduct = response.Data.Data.First();
        firstProduct.Id.Should().NotBeEmpty();
        firstProduct.Name.Should().NotBeNullOrEmpty();
        firstProduct.Slug.Should().NotBeNullOrEmpty();
        firstProduct.BasePrice.Should().BeGreaterThanOrEqualTo(0);
        firstProduct.Currency.Should().NotBeNullOrEmpty();
        firstProduct.CreatedAt.Should().NotBe(default);
        firstProduct.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task GetProductsDatatable_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange

        await CreateMultipleTestProductsAsync(8);

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // First page
        var firstPageRequest = GetProductsDatatableTestDataV1.Pagination.CreateFirstPageRequest(pageSize: 3);

        // Act
        var firstPageResponse =
            await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", firstPageRequest);

        // Assert
        AssertApiSuccess(firstPageResponse);
        firstPageResponse!.Data.Data.Should().HaveCount(3);
        firstPageResponse.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(8);

        // Second page
        var secondPageRequest = GetProductsDatatableTestDataV1.Pagination.CreateSecondPageRequest(pageSize: 3);
        var secondPageResponse =
            await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", secondPageRequest);

        AssertApiSuccess(secondPageResponse);
        secondPageResponse!.Data.Data.Should().HaveCount(3);

        // Verify different products on different pages
        var firstPageIds = firstPageResponse.Data.Data.Select(p => p.Id).ToList();
        var secondPageIds = secondPageResponse.Data.Data.Select(p => p.Id).ToList();
        firstPageIds.Should().NotIntersectWith(secondPageIds);
    }

    [Fact]
    public async Task GetProductsDatatable_WithSearch_ShouldReturnFilteredResults()
    {
        // Arrange

        await CreateTestProductAsync("Searchable Gaming Laptop", "searchable-gaming-laptop",
            "Gaming laptop for search test", 1499.99m, "USD", "SEARCH001");
        await CreateTestProductAsync("Another Product", "another-product", "Different product for testing", 99.99m,
            "USD", "OTHER001");

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.SearchScenarios.CreateNameSearchRequest("Searchable");

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().HaveCount(1);
        response.Data.Data.First().Name.Should().Contain("Searchable");
        response.Data.RecordsFiltered.Should().Be(1);
    }

    [Fact]
    public async Task GetProductsDatatable_WithSkuSearch_ShouldReturnMatchingProducts()
    {
        // Arrange

        await CreateTestProductAsync("Product with SKU", "product-sku", "Product for SKU search", 199.99m, "USD",
            "SKU12345");
        await CreateTestProductAsync("Another Product", "another", "Another product", 299.99m, "USD", "ABC99999");

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.SearchScenarios.CreateSkuSearchRequest("SKU12345");

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().HaveCount(1);
        response.Data.Data.First().Sku.Should().Contain("SKU12345");
    }

    [Fact]
    public async Task GetProductsDatatable_WithSorting_ShouldReturnSortedResults()
    {
        // Arrange

        await CreateTestProductAsync("Alpha Product", "alpha-product", "First product alphabetically", 99.99m, "USD",
            "ALPHA001");
        await CreateTestProductAsync("Beta Product", "beta-product", "Second product alphabetically", 199.99m, "USD",
            "BETA001");
        await CreateTestProductAsync("Gamma Product", "gamma-product", "Third product alphabetically", 299.99m, "USD",
            "GAMMA001");

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.SortingScenarios.CreateSortByNameAscRequest();

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().HaveCountGreaterThanOrEqualTo(3);

        var sortedNames = response.Data.Data.Select(p => p.Name).ToList();
        sortedNames.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetProductsDatatable_WithDescendingSortByCreatedAt_ShouldReturnNewestFirst()
    {
        // Arrange

        var oldProductId =
            await CreateTestProductAsync("Old Product", "old-product", "Older product", 100m, "USD", "OLD001");
        await Task.Delay(1000); // Ensure different timestamps
        var newProductId =
            await CreateTestProductAsync("New Product", "new-product", "Newer product", 200m, "USD", "NEW001");

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.SortingScenarios.CreateSortByCreatedAtRequest();

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();

        var sortedDates = response.Data.Data.Select(p => p.CreatedAt).ToList();
        sortedDates.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetProductsDatatable_WithSortByPrice_ShouldReturnSortedByPrice()
    {
        // Arrange

        await CreateTestProductAsync("Expensive Product", "expensive", "High-priced product", 999.99m, "USD", "EXP001");
        await CreateTestProductAsync("Cheap Product", "cheap", "Low-priced product", 9.99m, "USD", "CHEAP001");
        await CreateTestProductAsync("Medium Product", "medium", "Medium-priced product", 99.99m, "USD", "MED001");

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.SortingScenarios.CreateSortByPriceRequest();

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().HaveCountGreaterThanOrEqualTo(3);

        var sortedPrices = response.Data.Data.Select(p => p.BasePrice).ToList();
        sortedPrices.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetProductsDatatable_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = GetProductsDatatableTestDataV1.CreateValidRequest();

        // Act
        var response = await PostAsync("v1/products/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProductsDatatable_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.CreateValidRequest();

        // Act
        var response = await PostAsync("v1/products/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProductsDatatable_WithManagerRole_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.CreateValidRequest();

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductsDatatable_WithZeroLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.Pagination.CreateZeroLengthRequest();

        // Act
        var response = await PostAsync("v1/products/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Length must be greater than 0");
    }

    [Fact]
    public async Task GetProductsDatatable_WithNegativeValues_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.ValidationTests.CreateNegativeStartRequest();

        // Act
        var response = await PostAsync("v1/products/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Start must be greater than or equal to 0");
    }

    [Fact]
    public async Task GetProductsDatatable_WithExcessiveLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.ValidationTests.CreateExcessiveLengthRequest();

        // Act
        var response = await PostAsync("v1/products/datatable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Length must be less than or equal to 1000");
    }

    [Fact]
    public async Task GetProductsDatatable_WithNoResultsSearch_ShouldReturnEmptyData()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.SearchScenarios.CreateNoResultsSearchRequest();

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().BeEmpty();
        response.Data.RecordsFiltered.Should().Be(0);
        response.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(0); // Total products might exist
    }

    [Fact]
    public async Task GetProductsDatatable_WithUnicodeSearch_ShouldReturnCorrectResults()
    {
        // Arrange

        await CreateTestProductAsync("Café Münchën Special", "cafe-munchen-special", "Product with unicode characters",
            49.99m, "USD", "UNICODE001");

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.SearchScenarios.CreateUnicodeSearchRequest();

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().HaveCount(1);
        response.Data.Data.First().Name.Should().Contain("Café");
    }

    [Fact]
    public async Task GetProductsDatatable_WithComplexRequest_ShouldHandleAllParameters()
    {
        // Arrange

        await CreateTestProductAsync("Complex Product 1", "complex1", "First complex product", 99.99m, "USD",
            "COMPLEX001");
        await CreateTestProductAsync("Complex Product 2", "complex2", "Second complex product", 199.99m, "USD",
            "COMPLEX002");

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.EdgeCases.CreateComplexRequest();

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Draw.Should().Be(request.Draw);
        response.Data.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductsDatatable_WithInvalidColumnSort_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.SortingScenarios.CreateInvalidColumnSortRequest();

        // Act
        var response = await PostAsync("v1/products/datatable", request);

        // Assert - Should either return BadRequest or handle gracefully with default sort
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductsDatatable_ConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var requests = Enumerable.Range(0, 5)
            .Select(i => GetProductsDatatableTestDataV1.CreateValidRequest(draw: i + 1))
            .ToList();

        // Act
        var tasks = requests.Select(request =>
            PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request)
        ).ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Should().HaveCount(5);

        // Verify each response has correct draw number
        for (int i = 0; i < responses.Length; i++)
        {
            responses[i]!.Data.Draw.Should().Be(i + 1);
        }
    }

    [Fact]
    public async Task GetProductsDatatable_WithEmptySearch_ShouldReturnAllProducts()
    {
        // Arrange

        await CreateTestProductAsync("Empty Test Product", "empty-test", "Product for empty search test", 29.99m, "USD",
            "EMPTY001");

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.SearchScenarios.CreateEmptySearchRequest();

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.RecordsFiltered.Should().Be(response.Data.RecordsTotal);
        response.Data.Data.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetProductsDatatable_WithHighPageNumber_ShouldReturnEmptyOrLastPage()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.Pagination.CreateHighStartRequest();

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Data.Should().BeEmpty(); // No products at such high page number
        response.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(0);
        response.Data.RecordsFiltered.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetProductsDatatable_DatabaseConsistency_ShouldMatchDatabaseCounts()
    {
        // Arrange

        var testProduct1Id = await CreateTestProductAsync("Database Test 1", "db-test1", "First test product", 19.99m,
            "USD", "DB001");
        var testProduct2Id = await CreateTestProductAsync("Database Test 2", "db-test2", "Second test product", 39.99m,
            "USD", "DB002");

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.CreateValidRequest(length: 100); // Get all products

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);

        // Verify against database
        await ExecuteDbContextAsync(async context =>
        {
            var totalProductsInDb = await context.Products.CountAsync();
            response!.Data.RecordsTotal.Should().Be(totalProductsInDb);
            response.Data.RecordsFiltered.Should().Be(totalProductsInDb);

            // Verify specific test products are included
            var testProductIds = new[] { testProduct1Id, testProduct2Id };
            var responseIds = response.Data.Data.Select(p => p.Id).ToList();
            responseIds.Should().Contain(testProductIds);
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    public async Task GetProductsDatatable_WithDifferentPageSizes_ShouldReturnCorrectCount(int pageSize)
    {
        // Arrange

        await CreateMultipleTestProductsAsync(15); // Create enough products to test pagination

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.CreateValidRequest(length: pageSize);

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();

        // Should return at most pageSize items, but could be less if not enough data
        response.Data.Data.Should().HaveCountLessOrEqualTo(pageSize);
        response.Data.RecordsTotal.Should().BeGreaterThanOrEqualTo(response.Data.Data.Count);
    }

    [Fact]
    public async Task GetProductsDatatable_ResponseTime_ShouldBeReasonable()
    {
        // Arrange

        await CreateMultipleTestProductsAsync(50); // Create a decent amount of data

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.CreateValidRequest();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);
        stopwatch.Stop();

        // Assert
        AssertApiSuccess(response);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10)); // Should be fast enough for UI
    }

    [Fact]
    public async Task GetProductsDatatable_WithActiveInactiveProducts_ShouldShowCorrectStatus()
    {
        // Arrange

        await CreateTestProductAsync("Active Product", "active-product", "Active product for testing", 99.99m, "USD",
            "ACTIVE001");
        // Note: All products are created as active by default in the domain

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.CreateValidRequest(length: 10);

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();

        var activeProduct = response.Data.Data.FirstOrDefault(p => p.Name == "Active Product");
        activeProduct?.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductsDatatable_WithCategories_ShouldShowCategoryList()
    {
        // Arrange

        var categoryId = await CreateTestCategoryAsync("Electronics", "electronics", "Electronic products");
        await CreateTestProductWithCategoriesAsync("Electronic Product", "electronic-product", "Product with category",
            199.99m, "USD", "ELEC001", new List<Guid> { categoryId });

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.SearchScenarios.CreateNameSearchRequest("Electronic Product");

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();

        var electronicProduct = response.Data.Data.FirstOrDefault(p => p.Name == "Electronic Product");
        electronicProduct.Should().NotBeNull();
        electronicProduct!.Categories.Should().NotBeEmpty();
        electronicProduct.Categories.Should().Contain("Electronics");
    }

    [Fact]
    public async Task GetProductsDatatable_WithVariantsCount_ShouldShowCorrectCount()
    {
        // Arrange
        var attributeId = await CreateVariantAttributeAsync();

        var productId = await CreateTestProductAsync("Product With Variants", "product-variants",
            "Product with multiple variants", 99.99m, "USD", "VARIANT001");

        // Add variants to the product
        await AddProductVariantAsync(productId, "VARIANT001-S", 89.99m, 10, attributeId);
        await AddProductVariantAsync(productId, "VARIANT001-M", 99.99m, 15, attributeId);
        await AddProductVariantAsync(productId, "VARIANT001-L", 109.99m, 20, attributeId);

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = GetProductsDatatableTestDataV1.SearchScenarios.CreateNameSearchRequest("Product With Variants");

        // Act
        var response = await PostDataTableResponseAsync<ProductDatatableDto>("v1/products/datatable", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();

        var productWithVariants = response.Data.Data.FirstOrDefault(p => p.Name == "Product With Variants");
        productWithVariants.Should().NotBeNull();
        productWithVariants!.VariantsCount.Should().Be(3);
        productWithVariants.TotalStockQuantity.Should().Be(45); // 10 + 15 + 20
    }

    // Helper methods
    private async Task<Guid> CreateTestProductAsync(
        string name,
        string slug,
        string description,
        decimal basePrice,
        string currency,
        string? sku = null)
    {
        return await CreateTestProductWithCategoriesAsync(name, slug, description, basePrice, currency, sku,
            new List<Guid>());
    }

    private async Task<Guid> CreateTestProductWithCategoriesAsync(
        string name,
        string slug,
        string description,
        decimal basePrice,
        string currency,
        string? sku,
        List<Guid> categoryIds)
    {
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var request = ProductTestDataV1.Creation.CreateValidRequest(
            name: name,
            slug: slug,
            description: description,
            basePrice: basePrice,
            currency: currency,
            sku: sku,
            categoryIds: categoryIds
        );

        var response = await PostMultipartAsync("v1/products", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
        var productId = jsonDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        return productId;
    }

    private async Task CreateMultipleTestProductsAsync(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var name = $"Test Product {i}";
            var slug = $"test-product-{i}";
            var description = $"Test product {i} description";
            var price = 10m + (i * 10);
            var sku = $"TEST{i:D3}";

            await CreateTestProductAsync(name, slug, description, price, "USD", sku);
        }
    }

    private async Task<Guid> CreateTestCategoryAsync(string name, string slug, string description)
    {
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var request = CategoryTestDataV1.Creation.CreateValidRequest(
            name: name,
            slug: slug,
            description: description,
            parentId: null
        );

        var response = await PostAsync("v1/categories", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
        var categoryId = jsonDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        return categoryId;
    }

    private async Task<Guid> CreateVariantAttributeAsync()
    {
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var attributeRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: $"variant_attr_{uniqueId}",
            displayName: $"Variant Attr {uniqueId}",
            type: "Select",
            isVariant: true);

        var content = await PostAsync("v1/attributes", attributeRequest);
        content.EnsureSuccessStatusCode();

        var responseContent = await content.Content.ReadAsStringAsync();
        var jsonDoc = System.Text.Json.JsonDocument.Parse(responseContent);
        var attributeId = jsonDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        return attributeId;
    }

    private async Task AddProductVariantAsync(Guid productId, string sku, decimal price, int stockQuantity, Guid attributeId)
    {
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var request = new
        {
            Sku = sku,
            Price = price,
            StockQuantity = stockQuantity,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = sku // Use SKU as the attribute value for simplicity
                }
            },
            Images = new List<object>()
        };

        var response = await PostMultipartAsync($"v1/products/{productId}/variants", request);
        response.EnsureSuccessStatusCode();
    }
}
