using System.Net;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Administration.RebuildSearchIndex.V1;

public class RebuildSearchIndexEndpointV1Tests : ApiIntegrationTestBase
{
    public RebuildSearchIndexEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task RebuildSearchIndex_WithValidDataAsAdmin_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = RebuildSearchIndexTestDataV1.CreateValidRequest();

        // Act
        var response = await PostApiResponseAsync<object, RebuildSearchIndexResponseV1>(
            "v1/administration/search/rebuild", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.IsSuccess.Should().BeTrue();
        response.Data.Message.Should().NotBeNullOrEmpty();
        response.Data.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        response.Data.Duration.Should().BePositive();
    }

    [Fact]
    public async Task RebuildSearchIndex_WithDefaultRequest_ShouldInitializeAndIndexProducts()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = RebuildSearchIndexTestDataV1.CommonScenarios.CreateDefaultRequest();

        // Act
        var response = await PostApiResponseAsync<object, RebuildSearchIndexResponseV1>(
            "v1/administration/search/rebuild", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsSuccess.Should().BeTrue();
        response.Data.IndexesInitialized.Should().BeTrue();
        response.Data.ProductsIndexed.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task RebuildSearchIndex_WithInitializeOnlyRequest_ShouldInitializeButNotIndexProducts()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = RebuildSearchIndexTestDataV1.CommonScenarios.CreateInitializeOnlyRequest();

        // Act
        var response = await PostApiResponseAsync<object, RebuildSearchIndexResponseV1>(
            "v1/administration/search/rebuild", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsSuccess.Should().BeTrue();
        response.Data.IndexesInitialized.Should().BeTrue();
        response.Data.ProductsIndexed.Should().Be(0);
    }

    [Fact]
    public async Task RebuildSearchIndex_WithIndexProductsOnlyRequest_ShouldIndexProductsWithoutInitializing()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = RebuildSearchIndexTestDataV1.CommonScenarios.CreateIndexProductsOnlyRequest();

        // Act
        var response = await PostApiResponseAsync<object, RebuildSearchIndexResponseV1>(
            "v1/administration/search/rebuild", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsSuccess.Should().BeTrue();
        response.Data.IndexesInitialized.Should().BeFalse();
        response.Data.ProductsIndexed.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task RebuildSearchIndex_WithForceReindexRequest_ShouldForceReindexing()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = RebuildSearchIndexTestDataV1.CommonScenarios.CreateForceReindexRequest();

        // Act
        var response = await PostApiResponseAsync<object, RebuildSearchIndexResponseV1>(
            "v1/administration/search/rebuild", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsSuccess.Should().BeTrue();
        response.Data.IndexesInitialized.Should().BeTrue();
        response.Data.ProductsIndexed.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task RebuildSearchIndex_WithMinimalRequest_ShouldReturnSuccessWithMinimalAction()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = RebuildSearchIndexTestDataV1.CommonScenarios.CreateMinimalRequest();

        // Act
        var response = await PostApiResponseAsync<object, RebuildSearchIndexResponseV1>(
            "v1/administration/search/rebuild", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsSuccess.Should().BeTrue();
        response.Data.IndexesInitialized.Should().BeFalse();
        response.Data.ProductsIndexed.Should().Be(0);
    }

    [Fact]
    public async Task RebuildSearchIndex_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = RebuildSearchIndexTestDataV1.CreateValidRequest();

        // Act
        var response = await PostAsync("v1/administration/search/rebuild", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RebuildSearchIndex_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = RebuildSearchIndexTestDataV1.CreateValidRequest();

        // Act
        var response = await PostAsync("v1/administration/search/rebuild", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    [InlineData(true, true, true)]
    public async Task RebuildSearchIndex_WithDifferentParameterCombinations_ShouldReturnSuccess(
        bool initializeIndexes, bool indexProducts, bool forceReindex)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = RebuildSearchIndexTestDataV1.CreateValidRequest(
            initializeIndexes: initializeIndexes,
            indexProducts: indexProducts,
            forceReindex: forceReindex);

        // Act
        var response = await PostApiResponseAsync<object, RebuildSearchIndexResponseV1>(
            "v1/administration/search/rebuild", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsSuccess.Should().BeTrue();
        response.Data.IndexesInitialized.Should().Be(initializeIndexes);
        
        if (indexProducts)
        {
            response.Data.ProductsIndexed.Should().BeGreaterOrEqualTo(0);
        }
        else
        {
            response.Data.ProductsIndexed.Should().Be(0);
        }
    }

    [Fact]
    public async Task RebuildSearchIndex_WithAllFalseParameters_ShouldStillReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = RebuildSearchIndexTestDataV1.EdgeCases.CreateAllFalseRequest();

        // Act
        var response = await PostApiResponseAsync<object, RebuildSearchIndexResponseV1>(
            "v1/administration/search/rebuild", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsSuccess.Should().BeTrue();
        response.Data.IndexesInitialized.Should().BeFalse();
        response.Data.ProductsIndexed.Should().Be(0);
        response.Data.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RebuildSearchIndex_WithOnlyForceReindex_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = RebuildSearchIndexTestDataV1.EdgeCases.CreateOnlyForceReindexRequest();

        // Act
        var response = await PostApiResponseAsync<object, RebuildSearchIndexResponseV1>(
            "v1/administration/search/rebuild", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsSuccess.Should().BeTrue();
        response.Data.IndexesInitialized.Should().BeFalse();
        response.Data.ProductsIndexed.Should().Be(0);
    }

    [Fact]
    public async Task RebuildSearchIndex_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var tasks = Enumerable.Range(0, 3) // Reduced concurrent requests for search operations
            .Select(_ => RebuildSearchIndexTestDataV1.CreateValidRequest())
            .Select(request => PostApiResponseAsync<object, RebuildSearchIndexResponseV1>(
                "v1/administration/search/rebuild", request))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Should().AllSatisfy(response => 
            response!.Data.IsSuccess.Should().BeTrue());
    }

    [Fact]
    public async Task RebuildSearchIndex_ValidRequest_ShouldHaveReasonableResponseTime()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = RebuildSearchIndexTestDataV1.CreateValidRequest();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await PostApiResponseAsync<object, RebuildSearchIndexResponseV1>(
            "v1/administration/search/rebuild", request);
        stopwatch.Stop();

        // Assert
        AssertApiSuccess(response);
        response!.Data.Duration.Should().BeLessOrEqualTo(stopwatch.Elapsed);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMinutes(5)); // Reasonable timeout
    }

    // Response DTO for this specific endpoint version
    public class RebuildSearchIndexResponseV1
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
        public bool IndexesInitialized { get; set; }
        public int ProductsIndexed { get; set; }
        public int ProductsDeleted { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan Duration { get; set; }
    }
}