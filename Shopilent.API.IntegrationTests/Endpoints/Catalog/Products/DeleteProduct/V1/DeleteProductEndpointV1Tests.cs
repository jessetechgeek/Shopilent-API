using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;
using Shopilent.API.Endpoints.Catalog.Products.CreateProduct.V1;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Products.DeleteProduct.V1;

public class DeleteProductEndpointV1Tests : ApiIntegrationTestBase
{
    public DeleteProductEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task DeleteProduct_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product first
        var createRequest = ProductTestDataV1.Creation.CreateValidProductForDeletion();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
        apiResponse!.Data.Should().Be("Product deleted successfully");
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ShouldRemoveProductFromDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product first
        var createRequest = ProductTestDataV1.Creation.CreateValidProductForDeletion(
            name: "Delete Test Product DB",
            slug: "delete-test-product-db");
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Verify product exists
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);
            product.Should().NotBeNull();
        });

        // Act
        var response = await DeleteApiResponseAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);

        // Verify product no longer exists in database
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);
            product.Should().BeNull();
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteProduct_WithDifferentActiveStatus_ShouldReturnSuccess(bool isActive)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product
        var productName = $"Delete {(isActive ? "Active" : "Inactive")} Product {Guid.NewGuid():N}";
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: productName.Length > 50 ? productName[..50] : productName,
            isActive: isActive);
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteProduct_ActiveProduct_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = ProductTestDataV1.Creation.CreateValidRequest(isActive: true);
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteProduct_InactiveProduct_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = ProductTestDataV1.EdgeCases.CreateInactiveProductRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task DeleteProduct_WithEmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await DeleteAsync($"v1/products/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Product ID is required.");
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidGuidFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await DeleteAsync("v1/products/invalid-guid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteProduct_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("not found", "does not exist");
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task DeleteProduct_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var productId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteProduct_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var productId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteProduct_WithAdminRole_ShouldReturnNotFoundForNonExistentProduct()
    {
        // Arrange - Test that admin has permission, but product doesn't exist
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound); // Not forbidden, so admin has permission
    }

    #endregion

    #region Conflict/Business Rule Tests

    [Fact]
    public async Task DeleteProduct_AlreadyDeleted_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product
        var createRequest = ProductTestDataV1.Creation.CreateValidProductForDeletion();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Delete the product first time
        var firstDeleteResponse = await DeleteApiResponseAsync($"v1/products/{productId}");
        firstDeleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstDeleteContent = await firstDeleteResponse.Content.ReadAsStringAsync();
        var firstDeleteApiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(firstDeleteContent, JsonOptions);
        AssertApiSuccess(firstDeleteApiResponse);

        // Process outbox messages to ensure the deletion is fully processed
        await ProcessOutboxMessagesAsync();

        // Act - Try to delete again
        var response = await DeleteAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task DeleteProduct_WithUnicodeCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = ProductTestDataV1.EdgeCases.CreateRequestWithUnicodeCharacters();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteProduct_WithComplexMetadata_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = ProductTestDataV1.EdgeCases.CreateRequestWithComplexMetadata();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteProduct_WithMaximumNameLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = ProductTestDataV1.BoundaryTests.CreateRequestWithMaximumNameLength();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteProduct_WithSpecialCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = ProductTestDataV1.EdgeCases.CreateRequestWithSpecialCharacters();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task DeleteProduct_WithMinimumValidData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = ProductTestDataV1.BoundaryTests.CreateRequestWithMinimumValidData();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteProduct_WithMaximumValidData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = ProductTestDataV1.BoundaryTests.CreateRequestWithMaximumValidData();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    #endregion

    #region Currency-Specific Tests

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    public async Task DeleteProduct_WithDifferentCurrencies_ShouldReturnSuccess(string currency)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = ProductTestDataV1.Creation.CreateValidRequest(currency: currency);
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    #endregion

    #region Performance/Bulk Tests

    [Fact]
    public async Task DeleteProduct_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple products first
        var createTasks = Enumerable.Range(0, 5)
            .Select(i => ProductTestDataV1.Creation.CreateValidProductForDeletion(
                name: $"Concurrent Delete {i} {Guid.NewGuid():N}",
                slug: $"concurrent-delete-{i}-{Guid.NewGuid():N}"))
            .Select(request => PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request))
            .ToList();

        var createResponses = await Task.WhenAll(createTasks);
        createResponses.Should().AllSatisfy(response => AssertApiSuccess(response));

        // Act - Delete all products concurrently
        var deleteTasks = createResponses
            .Select(response => DeleteApiResponseAsync($"v1/products/{response!.Data.Id}"))
            .ToList();

        var deleteResponses = await Task.WhenAll(deleteTasks);

        // Assert
        deleteResponses.Should().AllSatisfy(response => response.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task DeleteProduct_SequentialDeletes_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple products
        var productIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var createRequest = ProductTestDataV1.Creation.CreateValidProductForDeletion(
                name: $"Sequential Delete {i} {Guid.NewGuid():N}",
                slug: $"sequential-delete-{i}-{Guid.NewGuid():N}");
            var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
            AssertApiSuccess(createResponse);
            productIds.Add(createResponse!.Data.Id);
        }

        // Act & Assert - Delete products sequentially
        foreach (var productId in productIds)
        {
            var response = await DeleteApiResponseAsync($"v1/products/{productId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
            AssertApiSuccess(apiResponse);
        }
    }

    #endregion

    #region Integration with Other Endpoints Tests

    [Fact]
    public async Task DeleteProduct_ThenAttemptToGetIt_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product
        var createRequest = ProductTestDataV1.Creation.CreateValidProductForDeletion();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Verify product can be retrieved
        var getResponse = await Client.GetAsync($"v1/products/{productId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify product exists in database before deletion
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);
            product.Should().NotBeNull("Product should exist before deletion");
        });

        // Act - Delete the product
        var deleteResponse = await DeleteApiResponseAsync($"v1/products/{productId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
        var deleteApiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(deleteContent, JsonOptions);
        AssertApiSuccess(deleteApiResponse);

        // Verify product no longer exists in database after deletion
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);
            product.Should().BeNull("Product should not exist after deletion");
        });

        // Process outbox messages to trigger cache invalidation event handlers
        // This ensures deterministic test behavior instead of relying on background processing
        await ProcessOutboxMessagesAsync();

        // Assert - Verify product can no longer be retrieved via API
        var getAfterDeleteResponse = await Client.GetAsync($"v1/products/{productId}");
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_ThenAttemptToUpdateIt_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product
        var createRequest = ProductTestDataV1.Creation.CreateValidProductForDeletion();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Delete the product
        var deleteResponse = await DeleteApiResponseAsync($"v1/products/{productId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
        var deleteApiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(deleteContent, JsonOptions);
        AssertApiSuccess(deleteApiResponse);

        // Process outbox messages to trigger cache invalidation event handlers
        // This ensures deterministic test behavior instead of relying on background processing
        await ProcessOutboxMessagesAsync();

        // Act - Try to update the deleted product using multipart
        var updateRequest = new
        {
            Name = "Updated Product Name",
            Slug = "updated-product-name",
            Description = "Updated Description",
            BasePrice = 99.99m,
            Currency = "USD"
        };
        var updateResponse = await PutMultipartAsync($"v1/products/{productId}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task DeleteProduct_SuccessResponse_ShouldHaveCorrectFormat()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = ProductTestDataV1.Creation.CreateValidProductForDeletion();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
        apiResponse.Should().NotBeNull();
        apiResponse!.Succeeded.Should().BeTrue();
        apiResponse.Data.Should().NotBeNullOrEmpty();
        apiResponse.Data.Should().Be("Product deleted successfully");
        apiResponse.Message.Should().NotBeNullOrEmpty();
        apiResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task DeleteProduct_ErrorResponse_ShouldHaveCorrectFormat()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        // The response should be in ApiResponse format
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        apiResponse.Should().NotBeNull();
        apiResponse!.Succeeded.Should().BeFalse();
        apiResponse.Message.Should().NotBeNullOrEmpty();
        apiResponse.StatusCode.Should().Be(404);
    }

    #endregion

}
