using System.Diagnostics;
using System.Net;
using System.Web;
using Shopilent.API.Endpoints.Search.UniversalSearch.V1;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;

namespace Shopilent.API.IntegrationTests.Endpoints.Search.UniversalSearch.V1;

public class UniversalSearchEndpointV1Tests : ApiIntegrationTestBase
{
    public UniversalSearchEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }
    // Override to add search initialization for this test class
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        // Initialize search indexes since this test class requires search functionality
        await InitializeSearchIndexesAsync();
    }

    #region Helper Methods

    /// <summary>
    /// Builds query string from request object for GET requests
    /// </summary>
    private static string BuildQueryString(object request)
    {
        // For UniversalSearch requests, we expect a simple object with FiltersBase64 property
        var filtersBase64Property = request.GetType().GetProperty("FiltersBase64");
        if (filtersBase64Property == null)
        {
            throw new InvalidOperationException("Request object must have a FiltersBase64 property");
        }

        var filtersBase64 = filtersBase64Property.GetValue(request)?.ToString();

        // Allow null/empty values for validation tests - the API should handle these gracefully
        if (string.IsNullOrEmpty(filtersBase64))
        {
            return "FiltersBase64=";
        }

        return $"FiltersBase64={HttpUtility.UrlEncode(filtersBase64)}";
    }

    /// <summary>
    /// Makes a GET request with validation error expectation
    /// </summary>
    private async Task<HttpResponseMessage> GetWithValidationErrorAsync(object request)
    {
        var queryString = BuildQueryString(request);
        return await Client.GetAsync($"v1/search?{queryString}");
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task UniversalSearch_WithValidBasicSearch_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Creation.CreateBasicSearchRequest("test product");
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNull();
        response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        response.Data.PageNumber.Should().Be(1);
        response.Data.PageSize.Should().Be(20);
        response.Data.Query.Should().Be("test product");
        response.Data.Facets.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithEmptySearchQuery_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.EdgeCases.CreateRequestWithEmptySearchQuery();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNull();
        response.Data.Query.Should().Be("");
    }

    [Fact]
    public async Task UniversalSearch_WithCategoryFilters_ShouldReturnSuccess()
    {
        // Arrange
        var categorySlugs = UniversalSearchTestDataV1.CreateValidCategorySlugs(2);
        var request = UniversalSearchTestDataV1.Creation.CreateCategoryFilteredRequest(categorySlugs);
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNull();
        response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task UniversalSearch_WithPriceFilters_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Creation.CreatePriceFilteredRequest(
            priceMin: 10.00m,
            priceMax: 100.00m);
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNull();
        response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task UniversalSearch_WithAttributeFilters_ShouldReturnSuccess()
    {
        // Arrange
        var attributeFilters = UniversalSearchTestDataV1.CreateValidAttributeFilters();
        var request = UniversalSearchTestDataV1.Creation.CreateAttributeFilteredRequest(attributeFilters);
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNull();
        response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task UniversalSearch_WithComprehensiveFilters_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Creation.CreateComprehensiveRequest();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNull();
        response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        response.Data.PageNumber.Should().Be(1);
        response.Data.PageSize.Should().Be(20);
        response.Data.Query.Should().Be("premium laptop computer");
    }

    #endregion

    #region Validation Tests - FiltersBase64

    [Fact]
    public async Task UniversalSearch_WithEmptyFiltersBase64_ShouldReturnValidationError()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Validation.CreateRequestWithEmptyFiltersBase64();

        // Act
        var response = await GetWithValidationErrorAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("FiltersBase64 is required");
    }

    [Fact]
    public async Task UniversalSearch_WithNullFiltersBase64_ShouldReturnValidationError()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Validation.CreateRequestWithNullFiltersBase64();

        // Act
        var response = await GetWithValidationErrorAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("FiltersBase64 is required");
    }

    [Fact]
    public async Task UniversalSearch_WithWhitespaceFiltersBase64_ShouldReturnValidationError()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Validation.CreateRequestWithWhitespaceFiltersBase64();

        // Act
        var response = await GetWithValidationErrorAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("FiltersBase64 is required");
    }

    [Fact]
    public async Task UniversalSearch_WithInvalidBase64_ShouldReturnValidationError()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Validation.CreateRequestWithInvalidBase64();

        // Act
        var response = await GetWithValidationErrorAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("valid base64 encoded string");
    }

    [Fact]
    public async Task UniversalSearch_WithInvalidJson_ShouldReturnValidationError()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Validation.CreateRequestWithInvalidJson();

        // Act
        var response = await GetWithValidationErrorAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("valid filter JSON", "Invalid JSON format");
    }

    [Fact]
    public async Task UniversalSearch_WithMalformedJson_ShouldReturnValidationError()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Validation.CreateRequestWithMalformedJson();

        // Act
        var response = await GetWithValidationErrorAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("valid filter JSON", "Invalid JSON format");
    }

    [Fact]
    public async Task UniversalSearch_WithInvalidCategorySlugs_ShouldReturnValidationError()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Validation.CreateRequestWithInvalidCategorySlugs();

        // Act
        var response = await GetWithValidationErrorAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("valid filter JSON");
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task UniversalSearch_WithValidPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Creation.CreatePaginatedRequest(
            pageNumber: 1,
            pageSize: 5);
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.PageNumber.Should().Be(1);
        response.Data.PageSize.Should().Be(5);
        response.Data.Items.Length.Should().BeLessOrEqualTo(5);
    }

    [Fact]
    public async Task UniversalSearch_WithSecondPage_ShouldReturnCorrectPageData()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Creation.CreatePaginatedRequest(
            pageNumber: 2,
            pageSize: 10);
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.PageNumber.Should().Be(2);
        response.Data.PageSize.Should().Be(10);
        response.Data.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task UniversalSearch_WithMinimumPageValues_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.BoundaryTests.CreateRequestWithMinimumPageValues();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.PageNumber.Should().Be(1);
        response.Data.PageSize.Should().Be(1);
        response.Data.Items.Length.Should().BeLessOrEqualTo(1);
    }

    [Fact]
    public async Task UniversalSearch_WithMaximumPageSize_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.BoundaryTests.CreateRequestWithMaximumPageSize();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.PageSize.Should().Be(100);
        response.Data.Items.Length.Should().BeLessOrEqualTo(100);
    }

    [Fact]
    public async Task UniversalSearch_WithLargePageNumber_ShouldReturnEmptyResults()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.BoundaryTests.CreateRequestWithLargePageNumber();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.PageNumber.Should().Be(9999);
        response.Data.Items.Should().BeEmpty();
        response.Data.HasNextPage.Should().BeFalse();
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public async Task UniversalSearch_WithRelevanceSort_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.SortingTests.CreateRequestWithRelevanceSort();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithNameSortAscending_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.SortingTests.CreateRequestWithNameSortAscending();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithNameSortDescending_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.SortingTests.CreateRequestWithNameSortDescending();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithPriceSortAscending_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.SortingTests.CreateRequestWithPriceSortAscending();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithPriceSortDescending_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.SortingTests.CreateRequestWithPriceSortDescending();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithDateSortAscending_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.SortingTests.CreateRequestWithDateSortAscending();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithDateSortDescending_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.SortingTests.CreateRequestWithDateSortDescending();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithInvalidSortBy_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.SortingTests.CreateRequestWithInvalidSortBy();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert - Invalid sort fields are typically handled gracefully by falling back to default
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    #endregion

    #region Filter Combination Tests

    [Fact]
    public async Task UniversalSearch_WithStockFilterOnly_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.EdgeCases.CreateRequestWithStockFilterOnly();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithActiveFilterOnly_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.EdgeCases.CreateRequestWithActiveFilterOnly();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithAllBooleanFiltersFalse_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.EdgeCases.CreateRequestWithAllBooleanFiltersFalse();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNull();
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task UniversalSearch_WithMinimumPrices_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.BoundaryTests.CreateRequestWithMinimumPrices();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithMaximumPrices_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.BoundaryTests.CreateRequestWithMaximumPrices();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithMaximumCategorySlugs_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.BoundaryTests.CreateRequestWithMaximumCategorySlugs();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithMaximumAttributeFilters_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.BoundaryTests.CreateRequestWithMaximumAttributeFilters();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNull();
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task UniversalSearch_WithUnicodeSearchQuery_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.EdgeCases.CreateRequestWithUnicodeSearchQuery();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Query.Should().Be("café münchën laptop™ 测试");
    }

    [Fact]
    public async Task UniversalSearch_WithSpecialCharactersSearchQuery_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.EdgeCases.CreateRequestWithSpecialCharactersSearchQuery();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Query.Should().Be("laptop @2024 50% off! #premium");
    }

    [Fact]
    public async Task UniversalSearch_WithLongSearchQuery_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.EdgeCases.CreateRequestWithLongSearchQuery();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Query.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UniversalSearch_WithEmptyAttributeFilterValues_ShouldReturnSuccess()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.EdgeCases.CreateRequestWithEmptyAttributeFilterValues();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithNoExpectedResults_ShouldReturnEmptyResults()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.EdgeCases.CreateRequestWithNoExpectedResults();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().BeEmpty();
        response.Data.TotalCount.Should().Be(0);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task UniversalSearch_WithoutAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader(); // Search endpoint allows anonymous access
        var request = UniversalSearchTestDataV1.Creation.CreateBasicSearchRequest();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithCustomerAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UniversalSearchTestDataV1.Creation.CreateBasicSearchRequest();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_WithAdminAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = UniversalSearchTestDataV1.Creation.CreateBasicSearchRequest();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    #endregion

    #region Response Structure Tests

    [Fact]
    public async Task UniversalSearch_ShouldReturnCorrectResponseStructure()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Creation.CreateBasicSearchRequest();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        var data = response!.Data;

        // Verify all required properties exist
        data.Should().NotBeNull();
        data.Items.Should().NotBeNull();
        data.Facets.Should().NotBeNull();
        data.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        data.PageNumber.Should().BeGreaterThan(0);
        data.PageSize.Should().BeGreaterThan(0);
        data.TotalPages.Should().BeGreaterThanOrEqualTo(0);
        data.HasPreviousPage.Should().Be(data.PageNumber > 1);
        data.HasNextPage.Should().Be(data.PageNumber < data.TotalPages);
        data.Query.Should().NotBeNull();
    }

    [Fact]
    public async Task UniversalSearch_ShouldReturnValidFacetsStructure()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Creation.CreateBasicSearchRequest();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        var facets = response!.Data.Facets;

        facets.Should().NotBeNull();
        // Facets structure validation will depend on SearchFacets implementation
    }

    [Fact]
    public async Task UniversalSearch_ShouldReturnValidProductSearchResults()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Creation.CreateBasicSearchRequest();
        var queryString = BuildQueryString(request);

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");

        // Assert
        AssertApiSuccess(response);
        var items = response!.Data.Items;

        items.Should().NotBeNull();

        // If there are items, validate their structure
        if (items.Length > 0)
        {
            foreach (var item in items)
            {
                item.Should().NotBeNull();
                // ProductSearchResultDto structure validation will depend on its implementation
            }
        }
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task UniversalSearch_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var requests = UniversalSearchTestDataV1.PerformanceTests.CreateMultipleConcurrentRequests(5);
        var tasks = requests.Select(request =>
        {
            var queryString = BuildQueryString(request);
            return GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");
        }).ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Should().AllSatisfy(response => response!.Data.Should().NotBeNull());
    }

    [Fact]
    public async Task UniversalSearch_WithComplexFilters_ShouldReturnWithinReasonableTime()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.PerformanceTests.CreateComplexFilterRequest();
        var queryString = BuildQueryString(request);
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await GetApiResponseAsync<UniversalSearchResponseV1>($"v1/search?{queryString}");
        stopwatch.Stop();

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
    }

    #endregion

    #region HTTP Method Tests

    [Fact]
    public async Task UniversalSearch_WithPostMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Creation.CreateBasicSearchRequest();

        // Act
        var response = await PostAsync("v1/search", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task UniversalSearch_WithPutMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        var request = UniversalSearchTestDataV1.Creation.CreateBasicSearchRequest();

        // Act
        var response = await PutAsync("v1/search", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task UniversalSearch_WithDeleteMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange

        // Act
        var response = await DeleteAsync("v1/search");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    #endregion
}
