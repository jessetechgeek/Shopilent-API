using System.Net;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;
using Shopilent.API.Common.Services;
using Shopilent.API.Endpoints.Catalog.Products.GetPaginatedProducts.V1;
using Shopilent.API.Endpoints.Catalog.Products.CreateProduct.V1;
using Shopilent.API.Endpoints.Catalog.Categories.CreateCategory.V1;
using Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;
using Shopilent.Application.Abstractions.Caching;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Products.GetPaginatedProducts.V1;

public class GetPaginatedProductsEndpointV1Tests : ApiIntegrationTestBase
{
    private readonly FilterEncodingService _filterEncodingService = new();

    public GetPaginatedProductsEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    // Override to add search initialization for this test class
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        // Initialize search indexes since this test class requires search functionality
        await InitializeSearchIndexesAsync(false);
    }

    #region Happy Path Tests

    [Fact]
    public async Task GetPaginatedProducts_WithDefaultFilters_ShouldReturnSuccess()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(5);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters();
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.PageNumber.Should().Be(1);
        response.Data.PageSize.Should().Be(10);
        response.Data.Items.Should().NotBeNull();
        response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(5);
        response.Message.Should().Be("Products retrieved successfully");
    }

    [Fact]
    public async Task GetPaginatedProducts_WithCustomPageSize_ShouldReturnCorrectPageSize()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(15);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageSize: 5);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.PageSize.Should().Be(5);
        response.Data.Items.Should().HaveCountLessOrEqualTo(5);
        response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(15);
    }

    [Fact]
    public async Task GetPaginatedProducts_WithMultiplePages_ShouldReturnDifferentResults()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(10);
        await ProcessOutboxAndRebuildSearchAsync();

        var firstPageFilters = CreateBasicFilters(pageNumber: 1, pageSize: 3);
        var firstPageBase64 = EncodeFilters(firstPageFilters);

        var secondPageFilters = CreateBasicFilters(pageNumber: 2, pageSize: 3);
        var secondPageBase64 = EncodeFilters(secondPageFilters);

        // Act
        var firstPageResponse =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(firstPageBase64)}");
        var secondPageResponse =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(secondPageBase64)}");

        // Assert
        AssertApiSuccess(firstPageResponse);
        AssertApiSuccess(secondPageResponse);

        firstPageResponse!.Data.Items.Should().HaveCountLessOrEqualTo(3);
        secondPageResponse!.Data.Items.Should().HaveCountLessOrEqualTo(3);

        // Pages should have different products
        var firstPageIds = firstPageResponse.Data.Items.Select(p => p.Id).ToList();
        var secondPageIds = secondPageResponse.Data.Items.Select(p => p.Id).ToList();
        firstPageIds.Should().NotIntersectWith(secondPageIds);

        // Pagination metadata
        firstPageResponse.Data.HasPreviousPage.Should().BeFalse();
        firstPageResponse.Data.HasNextPage.Should().BeTrue();
        secondPageResponse.Data.HasPreviousPage.Should().BeTrue();
        secondPageResponse.Data.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task GetPaginatedProducts_SortByNameAscending_ShouldReturnSortedResults()
    {
        // Arrange
        await CreateTestProductAsync("Zebra Product", 100m);
        await CreateTestProductAsync("Alpha Product", 200m);
        await CreateTestProductAsync("Beta Product", 150m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(sortColumn: "Name", sortDescending: false, pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCountGreaterThanOrEqualTo(3);
        var sortedNames = response.Data.Items.Select(p => p.Name).ToList();
        sortedNames.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetPaginatedProducts_SortByNameDescending_ShouldReturnReverseSortedResults()
    {
        // Arrange
        await CreateTestProductAsync("Alpha Sort", 100m);
        await CreateTestProductAsync("Beta Sort", 200m);
        await CreateTestProductAsync("Gamma Sort", 150m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(sortColumn: "Name", sortDescending: true, pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCountGreaterThanOrEqualTo(3);
        var sortedNames = response.Data.Items.Select(p => p.Name).ToList();
        sortedNames.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetPaginatedProducts_WithSearchQuery_ShouldFilterByNameDescriptionSku()
    {
        // Arrange
        await CreateTestProductAsync("Laptop Computer XYZ", 999m, sku: "LAPTOP-001");
        await CreateTestProductAsync("Desktop Computer ABC", 1299m, sku: "DESKTOP-001");
        await CreateTestProductAsync("Tablet Device", 499m, sku: "TABLET-001");
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(searchQuery: "Computer", pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCount(2);
        response.Data.Items.Should().OnlyContain(p => p.Name.Contains("Computer"));
    }

    [Fact]
    public async Task GetPaginatedProducts_WithPriceMinFilter_ShouldReturnProductsAboveMinPrice()
    {
        // Arrange
        await CreateTestProductAsync("Cheap Product", 50m);
        await CreateTestProductAsync("Mid Product", 150m);
        await CreateTestProductAsync("Expensive Product", 300m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(priceMin: 100m, pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCount(2);
        response.Data.Items.Should().OnlyContain(p => p.BasePrice >= 100m);
    }

    [Fact]
    public async Task GetPaginatedProducts_WithPriceMaxFilter_ShouldReturnProductsBelowMaxPrice()
    {
        // Arrange
        await CreateTestProductAsync("Low Price", 50m);
        await CreateTestProductAsync("Mid Price", 150m);
        await CreateTestProductAsync("High Price", 300m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(priceMax: 200m, pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCount(2);
        response.Data.Items.Should().OnlyContain(p => p.BasePrice <= 200m);
    }

    [Fact]
    public async Task GetPaginatedProducts_WithPriceRangeFilter_ShouldReturnProductsWithinRange()
    {
        // Arrange
        await CreateTestProductAsync("Product 1", 50m);
        await CreateTestProductAsync("Product 2", 150m);
        await CreateTestProductAsync("Product 3", 250m);
        await CreateTestProductAsync("Product 4", 350m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(priceMin: 100m, priceMax: 300m, pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCount(2);
        response.Data.Items.Should().OnlyContain(p => p.BasePrice >= 100m && p.BasePrice <= 300m);
    }

    [Fact]
    public async Task GetPaginatedProducts_WithCategorySlugFilter_ShouldReturnProductsInCategory()
    {
        // Arrange
        var electronicsId = await CreateTestCategoryAsync("Electronics Test", "electronics-test");
        var clothingId = await CreateTestCategoryAsync("Clothing Test", "clothing-test");

        await CreateTestProductAsync("Laptop", 999m, categoryIds: new List<Guid> { electronicsId });
        await CreateTestProductAsync("Phone", 699m, categoryIds: new List<Guid> { electronicsId });
        await CreateTestProductAsync("Shirt", 29m, categoryIds: new List<Guid> { clothingId });
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(categorySlugs: new[] { "electronics-test" }, pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCount(2);
        response.Data.Items.Should().OnlyContain(p =>
            p.Categories.Any(c => c.Slug == "electronics-test"));
    }

    [Fact]
    public async Task GetPaginatedProducts_WithMultipleCategorySlugs_ShouldReturnProductsInAnyCategory()
    {
        // Arrange
        var cat1Id = await CreateTestCategoryAsync("Category 1", "category-1");
        var cat2Id = await CreateTestCategoryAsync("Category 2", "category-2");
        var cat3Id = await CreateTestCategoryAsync("Category 3", "category-3");

        await CreateTestProductAsync("Product A", 100m, categoryIds: new List<Guid> { cat1Id });
        await CreateTestProductAsync("Product B", 200m, categoryIds: new List<Guid> { cat2Id });
        await CreateTestProductAsync("Product C", 300m, categoryIds: new List<Guid> { cat3Id });
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(categorySlugs: new[] { "category-1", "category-2" }, pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCount(2);
        response.Data.Items.Select(p => p.Name).Should().BeEquivalentTo(new[] { "Product A", "Product B" });
    }

    [Fact]
    public async Task GetPaginatedProducts_WithCombinedFilters_ShouldApplyAllFilters()
    {
        // Arrange
        var categoryId = await CreateTestCategoryAsync("Test Category", "test-category");

        await CreateTestProductAsync("Laptop Computer", 500m, categoryIds: new List<Guid> { categoryId });
        await CreateTestProductAsync("Desktop Computer", 1500m, categoryIds: new List<Guid> { categoryId });
        await CreateTestProductAsync("Tablet Device", 300m, categoryIds: new List<Guid> { categoryId });
        await CreateTestProductAsync("Phone", 800m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(
            searchQuery: "Computer",
            priceMin: 400m,
            priceMax: 1000m,
            categorySlugs: new[] { "test-category" },
            pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCount(1);
        response.Data.Items.First().Name.Should().Be("Laptop Computer");
        response.Data.Items.First().BasePrice.Should().Be(500m);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task GetPaginatedProducts_WithEmptyFiltersBase64_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync("v1/products?FiltersBase64=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("FiltersBase64 is required");
    }

    [Fact]
    public async Task GetPaginatedProducts_WithInvalidBase64String_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync("v1/products?FiltersBase64=invalid-base64-!!!!");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("valid base64 encoded string");
    }

    [Fact]
    public async Task GetPaginatedProducts_WithInvalidJsonStructure_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidJson = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{invalid json}"));

        // Act
        var response = await Client.GetAsync($"v1/products?FiltersBase64={Uri.EscapeDataString(invalidJson)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("valid filter JSON");
    }

    [Fact]
    public async Task GetPaginatedProducts_WithInvalidSlugFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var filters = CreateBasicFilters(categorySlugs: new[] { "Invalid Slug With Spaces!" });
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response = await Client.GetAsync($"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPaginatedProducts_WithUppercaseSlug_ShouldReturnBadRequest()
    {
        // Arrange
        var filters = CreateBasicFilters(categorySlugs: new[] { "UPPERCASE-SLUG" });
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response = await Client.GetAsync($"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPaginatedProducts_WithZeroPageNumber_ShouldReturnBadRequest()
    {
        // Arrange
        var filters = CreateBasicFilters(pageNumber: 0);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response = await Client.GetAsync($"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPaginatedProducts_WithNegativePageNumber_ShouldReturnBadRequest()
    {
        // Arrange
        var filters = CreateBasicFilters(pageNumber: -1);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response = await Client.GetAsync($"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPaginatedProducts_WithZeroPageSize_ShouldReturnBadRequest()
    {
        // Arrange
        var filters = CreateBasicFilters(pageSize: 0);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response = await Client.GetAsync($"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPaginatedProducts_WithNegativePageSize_ShouldReturnBadRequest()
    {
        // Arrange
        var filters = CreateBasicFilters(pageSize: -5);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response = await Client.GetAsync($"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPaginatedProducts_WithExcessivePageSize_ShouldReturnBadRequest()
    {
        // Arrange
        var filters = CreateBasicFilters(pageSize: 1001);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response = await Client.GetAsync($"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Response Structure Tests

    [Fact]
    public async Task GetPaginatedProducts_ResponseStructure_ShouldBeValid()
    {
        // Arrange
        await CreateTestProductAsync("Structure Test Product", 99.99m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters();
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();

        // Verify pagination metadata
        response.Data.PageNumber.Should().BeGreaterThan(0);
        response.Data.PageSize.Should().BeGreaterThan(0);
        response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        response.Data.TotalPages.Should().BeGreaterThanOrEqualTo(0);
        response.Data.Items.Should().NotBeNull();
        response.Data.Facets.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPaginatedProducts_PaginationMetadata_ShouldBeAccurate()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(7);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageSize: 3);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(7);
        response.Data.TotalPages.Should().BeGreaterThanOrEqualTo(3);
        response.Data.PageNumber.Should().Be(1);
        response.Data.PageSize.Should().Be(3);
        response.Data.HasPreviousPage.Should().BeFalse();
        response.Data.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetPaginatedProducts_FacetsStructure_ShouldBeValid()
    {
        // Arrange
        var categoryId = await CreateTestCategoryAsync("Facet Test Category", "facet-test-category");
        await CreateTestProductAsync("Facet Product", 100m, categoryIds: new List<Guid> { categoryId });
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters();
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Facets.Should().NotBeNull();
        response.Data.Facets.Categories.Should().NotBeNull();
        response.Data.Facets.Attributes.Should().NotBeNull();
        response.Data.Facets.PriceRange.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPaginatedProducts_ProductSearchResultDto_ShouldHaveCompleteStructure()
    {
        // Arrange
        await CreateTestProductAsync("Complete Structure Product", 199.99m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters();
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCountGreaterThanOrEqualTo(1);

        var product = response.Data.Items.First();
        product.Id.Should().NotBeEmpty();
        product.Name.Should().NotBeNullOrEmpty();
        product.Slug.Should().NotBeNullOrEmpty();
        product.BasePrice.Should().BeGreaterThanOrEqualTo(0);
        product.Categories.Should().NotBeNull();
        product.Attributes.Should().NotBeNull();
        product.Variants.Should().NotBeNull();
        product.Images.Should().NotBeNull();
        product.PriceRange.Should().NotBeNull();
        product.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        product.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task GetPaginatedProducts_WithNoResults_ShouldReturnEmptyStructure()
    {
        // Arrange - Search for non-existent product
        var filters = CreateBasicFilters(searchQuery: "nonexistentproduct12345xyz");
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().BeEmpty();
        response.Data.TotalCount.Should().Be(0);
        response.Data.TotalPages.Should().Be(0);
        response.Data.HasPreviousPage.Should().BeFalse();
        response.Data.HasNextPage.Should().BeFalse();
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task GetPaginatedProducts_InactiveProductsWithActiveOnlyTrue_ShouldExcludeInactiveProducts()
    {
        // Arrange
        await CreateTestProductAsync("Active Product", 100m, isActive: true);
        await CreateTestProductAsync("Inactive Product", 200m, isActive: false);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(activeOnly: true, pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCount(1);
        response.Data.Items.Should().OnlyContain(p => p.IsActive == true);
        response.Data.Items.First().Name.Should().Be("Active Product");
    }

    [Fact]
    public async Task GetPaginatedProducts_InactiveProductsWithActiveOnlyFalse_ShouldIncludeInactiveProducts()
    {
        // Arrange
        await CreateTestProductAsync("Active Product Test", 100m, isActive: true);
        await CreateTestProductAsync("Inactive Product Test", 200m, isActive: false);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(activeOnly: false, pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCount(2);
        response.Data.Items.Should().Contain(p => p.Name == "Active Product Test" && p.IsActive == true);
        response.Data.Items.Should().Contain(p => p.Name == "Inactive Product Test" && p.IsActive == false);
    }

    [Fact]
    public async Task GetPaginatedProducts_DatabaseConsistency_ShouldMatchDatabaseCount()
    {
        // Arrange
        var product1Id = await CreateTestProductAsync("DB Test 1", 100m);
        var product2Id = await CreateTestProductAsync("DB Test 2", 200m);
        var product3Id = await CreateTestProductAsync("DB Test 3", 300m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageSize: 100);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);

        await ExecuteDbContextAsync(async context =>
        {
            var totalProductsInDb = await context.Products.CountAsync(p => p.IsActive);
            response!.Data.TotalCount.Should().Be(totalProductsInDb);

            var testProductIds = new[] { product1Id, product2Id, product3Id };
            var responseIds = response.Data.Items.Select(p => p.Id).ToList();
            responseIds.Should().Contain(testProductIds);
        });
    }

    [Fact]
    public async Task GetPaginatedProducts_WithCategoryFilter_ShouldOnlyReturnProductsInCategory()
    {
        // Arrange
        var electronicsId = await CreateTestCategoryAsync("Electronics", "electronics");
        var booksId = await CreateTestCategoryAsync("Books", "books");

        await CreateTestProductAsync("Laptop", 999m, categoryIds: new List<Guid> { electronicsId });
        await CreateTestProductAsync("Novel", 19.99m, categoryIds: new List<Guid> { booksId });
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(categorySlugs: new[] { "electronics" }, pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCount(1);
        response.Data.Items.First().Name.Should().Be("Laptop");
        response.Data.Items.First().Categories.Should().Contain(c => c.Slug == "electronics");
    }

    [Fact]
    public async Task GetPaginatedProducts_SearchQueryMatchesNameDescriptionSku_ShouldReturnAllMatches()
    {
        // Arrange
        await CreateTestProductAsync("Special Widget", 100m, sku: "WIDGET-001",
            description: "A special widget for testing");
        await CreateTestProductAsync("Regular Product", 200m, sku: "SPECIAL-002", description: "Regular description");
        await CreateTestProductAsync("Another Item", 300m, sku: "ITEM-003", description: "Special features included");
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(searchQuery: "Special", pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCount(3);
        response.Data.Items.Should().Contain(p => p.Name.Contains("Special"));
        response.Data.Items.Should().Contain(p => p.SKU.Contains("SPECIAL"));
        response.Data.Items.Should().Contain(p => p.Description.Contains("Special"));
    }

    [Fact]
    public async Task GetPaginatedProducts_PriceFilterAccuracy_ShouldOnlyReturnProductsInRange()
    {
        // Arrange
        await CreateTestProductAsync("Product $50", 50m);
        await CreateTestProductAsync("Product $100", 100m);
        await CreateTestProductAsync("Product $150", 150m);
        await CreateTestProductAsync("Product $200", 200m);
        await CreateTestProductAsync("Product $250", 250m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(priceMin: 100m, priceMax: 200m, pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCount(3);
        response.Data.Items.Should().OnlyContain(p => p.BasePrice >= 100m && p.BasePrice <= 200m);
    }

    [Fact]
    public async Task GetPaginatedProducts_CategoryFacets_ShouldShowCorrectProductCounts()
    {
        // Arrange
        var cat1Id = await CreateTestCategoryAsync("Category One", "category-one");
        var cat2Id = await CreateTestCategoryAsync("Category Two", "category-two");

        await CreateTestProductAsync("Product A", 100m, categoryIds: new List<Guid> { cat1Id });
        await CreateTestProductAsync("Product B", 200m, categoryIds: new List<Guid> { cat1Id });
        await CreateTestProductAsync("Product C", 300m, categoryIds: new List<Guid> { cat2Id });
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Facets.Categories.Should().NotBeEmpty();

        var cat1Facet = response.Data.Facets.Categories.FirstOrDefault(c => c.Slug == "category-one");
        var cat2Facet = response.Data.Facets.Categories.FirstOrDefault(c => c.Slug == "category-two");

        cat1Facet.Should().NotBeNull();
        cat1Facet!.Count.Should().Be(2);

        cat2Facet.Should().NotBeNull();
        cat2Facet!.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetPaginatedProducts_PriceRangeFacet_ShouldShowMinMaxFromAllProducts()
    {
        // Arrange
        await CreateTestProductAsync("Cheap", 25m);
        await CreateTestProductAsync("Mid", 150m);
        await CreateTestProductAsync("Expensive", 500m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Facets.PriceRange.Should().NotBeNull();
        response.Data.Facets.PriceRange.Min.Should().Be(25m);
        response.Data.Facets.PriceRange.Max.Should().Be(500m);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task GetPaginatedProducts_FirstPage_ShouldHaveNoPreviousPage()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(10);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageNumber: 1, pageSize: 3);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.HasPreviousPage.Should().BeFalse();
        response.Data.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task GetPaginatedProducts_LastPage_ShouldHaveNoNextPage()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(5);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageNumber: 1, pageSize: 10);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.HasNextPage.Should().BeFalse();
        response.Data.Items.Should().HaveCountLessOrEqualTo(5);
    }

    [Fact]
    public async Task GetPaginatedProducts_MiddlePage_ShouldHaveBothPreviousAndNextPage()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(15);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageNumber: 2, pageSize: 5);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.PageNumber.Should().Be(2);
        response.Data.HasPreviousPage.Should().BeTrue();
        response.Data.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetPaginatedProducts_DifferentPageSizes_ShouldReturnCorrectItemCounts()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(20);
        await ProcessOutboxAndRebuildSearchAsync();

        var sizes = new[] { 5, 10, 20 };

        foreach (var size in sizes)
        {
            var filters = CreateBasicFilters(pageSize: size);
            var filtersBase64 = EncodeFilters(filters);

            // Act
            var response =
                await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                    $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

            // Assert
            AssertApiSuccess(response);
            response!.Data.Items.Should().HaveCountLessOrEqualTo(size);
            response.Data.PageSize.Should().Be(size);
        }
    }

    [Fact]
    public async Task GetPaginatedProducts_PageBeyondTotalPages_ShouldReturnEmptyResults()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(5);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageNumber: 999, pageSize: 10);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().BeEmpty();
        response.Data.PageNumber.Should().Be(999);
        response.Data.HasPreviousPage.Should().BeTrue();
        response.Data.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetPaginatedProducts_TotalCountConsistentAcrossPages_ShouldRemainSame()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(10);
        await ProcessOutboxAndRebuildSearchAsync();

        var page1Filters = CreateBasicFilters(pageNumber: 1, pageSize: 3);
        var page1Base64 = EncodeFilters(page1Filters);

        var page2Filters = CreateBasicFilters(pageNumber: 2, pageSize: 3);
        var page2Base64 = EncodeFilters(page2Filters);

        // Act
        var page1Response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(page1Base64)}");
        var page2Response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(page2Base64)}");

        // Assert
        AssertApiSuccess(page1Response);
        AssertApiSuccess(page2Response);
        page1Response!.Data.TotalCount.Should().Be(page2Response!.Data.TotalCount);
    }

    [Fact]
    public async Task GetPaginatedProducts_TotalPagesCalculation_ShouldBeAccurate()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(17);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageSize: 5);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.TotalCount.Should().BeGreaterThanOrEqualTo(17);
        // ceil(17/5) = 4
        var expectedPages = (int)Math.Ceiling((double)response.Data.TotalCount / 5);
        response.Data.TotalPages.Should().Be(expectedPages);
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public async Task GetPaginatedProducts_SortByPriceAscending_ShouldReturnLowestPriceFirst()
    {
        // Arrange
        await CreateTestProductAsync("Product A", 300m);
        await CreateTestProductAsync("Product B", 100m);
        await CreateTestProductAsync("Product C", 200m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(sortColumn: "BasePrice", sortDescending: false, pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCountGreaterThanOrEqualTo(3);
        var sortedPrices = response.Data.Items.Select(p => p.BasePrice).ToList();
        sortedPrices.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetPaginatedProducts_SortByPriceDescending_ShouldReturnHighestPriceFirst()
    {
        // Arrange
        await CreateTestProductAsync("Product X", 100m);
        await CreateTestProductAsync("Product Y", 300m);
        await CreateTestProductAsync("Product Z", 200m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(sortColumn: "BasePrice", sortDescending: true, pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCountGreaterThanOrEqualTo(3);
        var sortedPrices = response.Data.Items.Select(p => p.BasePrice).ToList();
        sortedPrices.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetPaginatedProducts_SortByCreatedAtDescending_ShouldReturnNewestFirst()
    {
        // Arrange
        var oldProductId = await CreateTestProductAsync("Old Product", 100m);
        await Task.Delay(1000);
        var newProductId = await CreateTestProductAsync("New Product", 200m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(sortColumn: "CreatedAt", sortDescending: true, pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        var sortedDates = response.Data.Items.Select(p => p.CreatedAt).ToList();
        sortedDates.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetPaginatedProducts_SortOrderMaintainedAcrossPages_ShouldBeConsistent()
    {
        // Arrange
        for (int i = 1; i <= 10; i++)
        {
            await CreateTestProductAsync($"Product {i:D2}", i * 10m);
        }

        await ProcessOutboxAndRebuildSearchAsync();

        var page1Filters = CreateBasicFilters(pageNumber: 1, pageSize: 5, sortColumn: "Name", sortDescending: false);
        var page1Base64 = EncodeFilters(page1Filters);

        var page2Filters = CreateBasicFilters(pageNumber: 2, pageSize: 5, sortColumn: "Name", sortDescending: false);
        var page2Base64 = EncodeFilters(page2Filters);

        // Act
        var page1Response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(page1Base64)}");
        var page2Response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(page2Base64)}");

        // Assert
        AssertApiSuccess(page1Response);
        AssertApiSuccess(page2Response);

        var allNames = page1Response!.Data.Items.Select(p => p.Name)
            .Concat(page2Response!.Data.Items.Select(p => p.Name))
            .ToList();
        allNames.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetPaginatedProducts_DefaultSort_ShouldUseSensibleDefault()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(5);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters();
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().NotBeEmpty();
        // Verify default sort is applied (typically by Name ascending)
        response.Data.Items.Should().BeInAscendingOrder(p => p.Name);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetPaginatedProducts_WithUnicodeCharacters_ShouldReturnCorrectly()
    {
        // Arrange
        await CreateTestProductAsync("Café Münchën Product™", 99m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(searchQuery: "Café", pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCount(1);
        response.Data.Items.First().Name.Should().Be("Café Münchën Product™");
    }

    [Fact]
    public async Task GetPaginatedProducts_ProductWithMultipleCategories_ShouldAppearInAllCategoryFacets()
    {
        // Arrange
        var cat1Id = await CreateTestCategoryAsync("Cat 1", "cat-1");
        var cat2Id = await CreateTestCategoryAsync("Cat 2", "cat-2");

        await CreateTestProductAsync("Multi-Category Product", 100m, categoryIds: new List<Guid> { cat1Id, cat2Id });
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.First().Categories.Should().HaveCount(2);
        response.Data.Facets.Categories.Should().Contain(c => c.Slug == "cat-1");
        response.Data.Facets.Categories.Should().Contain(c => c.Slug == "cat-2");
    }

    [Fact]
    public async Task GetPaginatedProducts_ProductWithoutCategories_ShouldStillBeReturned()
    {
        // Arrange
        await CreateTestProductAsync("No Category Product", 100m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().Contain(p => p.Name == "No Category Product");
        response.Data.Items.First(p => p.Name == "No Category Product").Categories.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPaginatedProducts_LargePageSize_ShouldReturnAllAvailableProducts()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(15);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageSize: 100);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCountGreaterThanOrEqualTo(15);
        response.Data.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task GetPaginatedProducts_EmptyDatabase_ShouldReturnEmptyResultsWithZeroCounts()
    {
        // Arrange - No products created
        await ClearProductCacheAsync();
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters();
        var filtersBase64 = EncodeFilters(filters);


        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().BeEmpty();
        response.Data.TotalCount.Should().Be(0);
        response.Data.TotalPages.Should().Be(0);
        response.Data.HasPreviousPage.Should().BeFalse();
        response.Data.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetPaginatedProducts_SingleProduct_ShouldReturnCorrectPagination()
    {
        // Arrange
        await CreateTestProductAsync("Only Product", 100m);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageSize: 10);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().HaveCount(1);
        response.Data.TotalCount.Should().Be(1);
        response.Data.TotalPages.Should().Be(1);
        response.Data.HasPreviousPage.Should().BeFalse();
        response.Data.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetPaginatedProducts_ComplexMetadata_ShouldBeIncluded()
    {
        // Arrange
        var productRequest = ProductTestDataV1.EdgeCases.CreateRequestWithComplexMetadata();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        await ProcessOutboxAndRebuildSearchAsync();
        ClearAuthenticationHeader();

        var filters = CreateBasicFilters(pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Items.Should().NotBeEmpty();
    }

    #endregion

    #region Performance & Caching Tests

    [Fact]
    public async Task GetPaginatedProducts_ConcurrentRequests_ShouldReturnConsistentData()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(10);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageSize: 10);
        var filtersBase64 = EncodeFilters(filters);

        var requests = Enumerable.Range(0, 5)
            .Select(_ =>
                GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                    $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}"))
            .ToList();

        // Act
        var responses = await Task.WhenAll(requests);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        var firstResponse = responses[0];
        responses.Should().AllSatisfy(response =>
        {
            response!.Data.TotalCount.Should().Be(firstResponse!.Data.TotalCount);
            response.Data.TotalPages.Should().Be(firstResponse.Data.TotalPages);
        });
    }

    [Fact]
    public async Task GetPaginatedProducts_CacheBehavior_ShouldCacheIdenticalRequests()
    {
        // Arrange
        await ClearProductCacheAsync(); // Clear cache to ensure fresh test
        await CreateMultipleTestProductsAsync(5);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters();
        var filtersBase64 = EncodeFilters(filters);
        var url = $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}";

        // Act - First request
        var firstResponse = await GetApiResponseAsync<GetPaginatedProductsResponseV1>(url);

        // Act - Second request (should hit cache)
        var secondResponse = await GetApiResponseAsync<GetPaginatedProductsResponseV1>(url);

        // Assert
        AssertApiSuccess(firstResponse);
        AssertApiSuccess(secondResponse);

        // Data should be identical (cached)
        firstResponse!.Data.TotalCount.Should().Be(secondResponse!.Data.TotalCount);
        firstResponse.Data.Items.Length.Should().Be(secondResponse.Data.Items.Length);
    }

    [Fact]
    public async Task GetPaginatedProducts_WithManyProducts_ShouldPerformReasonably()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(50);
        await ProcessOutboxAndRebuildSearchAsync();

        var filters = CreateBasicFilters(pageSize: 20);
        var filtersBase64 = EncodeFilters(filters);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");
        stopwatch.Stop();

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10)); // Should be reasonably fast
    }

    #endregion

    #region Anonymous Access Tests

    [Fact]
    public async Task GetPaginatedProducts_WithoutAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(3);
        await ProcessOutboxAndRebuildSearchAsync();
        ClearAuthenticationHeader();

        var filters = CreateBasicFilters();
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPaginatedProducts_WithCustomerRole_ShouldReturnSuccess()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(3);
        await ProcessOutboxAndRebuildSearchAsync();

        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);

        var filters = CreateBasicFilters();
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPaginatedProducts_WithAdminRole_ShouldReturnSuccess()
    {
        // Arrange
        await CreateMultipleTestProductsAsync(3);
        await ProcessOutboxAndRebuildSearchAsync();

        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var filters = CreateBasicFilters();
        var filtersBase64 = EncodeFilters(filters);

        // Act
        var response =
            await GetApiResponseAsync<GetPaginatedProductsResponseV1>(
                $"v1/products?FiltersBase64={Uri.EscapeDataString(filtersBase64)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private async Task ClearProductCacheAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        await cacheService.RemoveByPatternAsync("products-*");
    }

    private async Task RebuildSearchIndexAsync()
    {
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var request = new { InitializeIndexes = true, IndexProducts = true, ForceReindex = true };

        await PostAsync("v1/administration/search/rebuild", request);
        ClearAuthenticationHeader();

        await Task.Delay(200);
    }

    private async Task ProcessOutboxAndRebuildSearchAsync()
    {
        await ProcessOutboxMessagesAsync();
        await RebuildSearchIndexAsync();
    }

    private string EncodeFilters(ProductFilters filters)
    {
        return _filterEncodingService.EncodeFilters(filters);
    }

    private ProductFilters CreateBasicFilters(
        int pageNumber = 1,
        int pageSize = 10,
        string sortColumn = "Name",
        bool sortDescending = false,
        bool activeOnly = true,
        string? searchQuery = null,
        decimal? priceMin = null,
        decimal? priceMax = null,
        string[]? categorySlugs = null,
        bool inStockOnly = false)
    {
        return new ProductFilters
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortColumn = sortColumn,
            SortDescending = sortDescending,
            ActiveOnly = activeOnly,
            SearchQuery = searchQuery ?? "",
            CategorySlugs = categorySlugs ?? [],
            AttributeFilters = new Dictionary<string, string[]>(),
            PriceMin = priceMin,
            PriceMax = priceMax,
            InStockOnly = inStockOnly
        };
    }

    private async Task<Guid> CreateTestProductAsync(
        string name,
        decimal basePrice,
        List<Guid>? categoryIds = null,
        string? sku = null,
        string? description = null,
        bool isActive = true)
    {
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var request = ProductTestDataV1.Creation.CreateValidRequest(
            name: name,
            basePrice: basePrice,
            categoryIds: categoryIds,
            sku: sku,
            description: description,
            isActive: isActive);

        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);
        AssertApiSuccess(response);

        ClearAuthenticationHeader();
        return response!.Data.Id;
    }

    private async Task CreateMultipleTestProductsAsync(int count)
    {
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var products = ProductTestDataV1.Creation.CreateMultipleProductsForSeeding(count);

        foreach (var product in products)
        {
            await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", product);
        }

        ClearAuthenticationHeader();
    }

    private async Task<Guid> CreateTestCategoryAsync(string name, string slug, string? description = null)
    {
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var request = CategoryTestDataV1.Creation.CreateValidRequest(
            name: name,
            slug: slug,
            description: description);

        var response = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);
        AssertApiSuccess(response);

        ClearAuthenticationHeader();
        return response!.Data.Id;
    }

    private async Task<Guid> CreateTestAttributeAsync(string name, string displayName, string type = "Text")
    {
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var request = AttributeTestDataV1.Creation.CreateValidRequest(
            name: name,
            displayName: displayName,
            type: type);

        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);
        AssertApiSuccess(response);

        ClearAuthenticationHeader();
        return response!.Data.Id;
    }

    #endregion
}
