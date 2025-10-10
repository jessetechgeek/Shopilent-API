using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;
using Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;
using Shopilent.API.Endpoints.Catalog.Products.CreateProduct.V1;
using Shopilent.Application.Features.Catalog.Commands.AddProductVariant.V1;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Products.DeleteProductVariant.V1;

public class DeleteProductVariantEndpointV1Tests : ApiIntegrationTestBase
{
    public DeleteProductVariantEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Helper Methods

    private async Task<(Guid ProductId, Guid VariantId, Guid AttributeId)> CreateProductWithVariantAsync(
        string? variantSku = null,
        decimal? variantPrice = null,
        int? stockQuantity = null,
        bool? isActive = null)
    {
        // Create an attribute first (for variant attributes)
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var attributeRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: $"variant_attr_{uniqueId}",
            displayName: $"Variant Attr {uniqueId}",
            type: "Select",
            isVariant: true);
        var attributeResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);
        AssertApiSuccess(attributeResponse);
        var attributeId = attributeResponse!.Data.Id;

        // Create a product with unique slug
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: $"Test Product For Variant Delete {uniqueId}",
            slug: $"test-product-variant-delete-{uniqueId}");
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);
        var productId = productResponse!.Data.Id;

        // Add a variant to the product
        var variantRequest = new
        {
            Sku = variantSku ?? $"VAR-DEL-{uniqueId}",
            Price = variantPrice ?? 99.99m,
            StockQuantity = stockQuantity ?? 10,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Test Size"
                }
            },
            IsActive = isActive ?? true
        };

        var variantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>(
            $"v1/products/{productId}/variants", variantRequest);
        AssertApiSuccess(variantResponse);
        var variantId = variantResponse!.Data.Id;

        return (productId, variantId, attributeId);
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task DeleteProductVariant_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, _) = await CreateProductWithVariantAsync();

        // Act
        var response = await DeleteApiResponseAsync($"v1/variants/{variantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
        apiResponse!.Data.Should().Be("Product variant deleted successfully");
    }

    [Fact]
    public async Task DeleteProductVariant_WithValidId_ShouldRemoveVariantFromDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, _) = await CreateProductWithVariantAsync(
            variantSku: "DELETE-TEST-DB-001");

        // Verify variant exists before deletion
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId);
            variant.Should().NotBeNull();
        });

        // Act
        var response = await DeleteApiResponseAsync($"v1/variants/{variantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);

        // Verify variant no longer exists in database
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId);
            variant.Should().BeNull();
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteProductVariant_WithDifferentActiveStatus_ShouldReturnSuccess(bool isActive)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, _) = await CreateProductWithVariantAsync(isActive: isActive);

        // Act
        var response = await DeleteApiResponseAsync($"v1/variants/{variantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteProductVariant_ActiveVariant_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, _) = await CreateProductWithVariantAsync(isActive: true);

        // Act
        var response = await DeleteApiResponseAsync($"v1/variants/{variantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteProductVariant_InactiveVariant_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, _) = await CreateProductWithVariantAsync(isActive: false);

        // Act
        var response = await DeleteApiResponseAsync($"v1/variants/{variantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteProductVariant_WithZeroStock_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, _) = await CreateProductWithVariantAsync(stockQuantity: 0);

        // Act
        var response = await DeleteApiResponseAsync($"v1/variants/{variantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteProductVariant_WithHighStock_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, _) = await CreateProductWithVariantAsync(stockQuantity: 9999);

        // Act
        var response = await DeleteApiResponseAsync($"v1/variants/{variantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task DeleteProductVariant_WithEmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await DeleteAsync($"v1/variants/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Product variant ID is required.");
    }

    [Fact]
    public async Task DeleteProductVariant_WithInvalidGuidFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await DeleteAsync("v1/variants/invalid-guid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteProductVariant_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/variants/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("not found", "was not found");
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task DeleteProductVariant_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var variantId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/variants/{variantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteProductVariant_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var variantId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/variants/{variantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteProductVariant_WithAdminRole_ShouldReturnNotFoundForNonExistentVariant()
    {
        // Arrange - Test that admin has permission, but variant doesn't exist
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/variants/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound); // Not forbidden, so admin has permission
    }

    [Fact]
    public async Task DeleteProductVariant_WithManagerRole_ShouldReturnSuccess()
    {
        // Arrange
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var (productId, variantId, _) = await CreateProductWithVariantAsync();

        // Re-authenticate as manager
        var managerToken = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(managerToken);

        // Act
        var response = await DeleteApiResponseAsync($"v1/variants/{variantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    #endregion

    #region Conflict/Business Rule Tests

    [Fact]
    public async Task DeleteProductVariant_AlreadyDeleted_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, _) = await CreateProductWithVariantAsync();

        // Delete the variant first time
        var firstDeleteResponse = await DeleteApiResponseAsync($"v1/variants/{variantId}");
        firstDeleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstDeleteContent = await firstDeleteResponse.Content.ReadAsStringAsync();
        var firstDeleteApiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(firstDeleteContent, JsonOptions);
        AssertApiSuccess(firstDeleteApiResponse);

        // Process outbox messages to ensure the deletion is fully processed
        await ProcessOutboxMessagesAsync();

        // Act - Try to delete again
        var response = await DeleteAsync($"v1/variants/{variantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task DeleteProductVariant_WithVariousStockLevels_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var stockLevels = new[] { 0, 1, 100, 9999 };
        var variantIds = new List<Guid>();

        foreach (var stockLevel in stockLevels)
        {
            var (_, variantId, _) = await CreateProductWithVariantAsync(stockQuantity: stockLevel);
            variantIds.Add(variantId);
        }

        // Act & Assert
        foreach (var variantId in variantIds)
        {
            var response = await DeleteApiResponseAsync($"v1/variants/{variantId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
            AssertApiSuccess(apiResponse);
        }
    }

    [Fact]
    public async Task DeleteProductVariant_WithVariousPrices_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var prices = new[] { 0.01m, 1.00m, 99.99m, 9999.99m };
        var variantIds = new List<Guid>();

        foreach (var price in prices)
        {
            var (_, variantId, _) = await CreateProductWithVariantAsync(variantPrice: price);
            variantIds.Add(variantId);
        }

        // Act & Assert
        foreach (var variantId in variantIds)
        {
            var response = await DeleteApiResponseAsync($"v1/variants/{variantId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
            AssertApiSuccess(apiResponse);
        }
    }

    [Fact]
    public async Task DeleteProductVariant_WithSpecialCharactersInSku_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, _) = await CreateProductWithVariantAsync(
            variantSku: "VAR-SKU_TEST@2024!");

        // Act
        var response = await DeleteApiResponseAsync($"v1/variants/{variantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    #endregion

    #region Performance/Bulk Tests

    [Fact]
    public async Task DeleteProductVariant_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple variants
        var variantIds = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var (_, variantId, _) = await CreateProductWithVariantAsync();
            variantIds.Add(variantId);
        }

        // Act - Delete all variants concurrently
        var deleteTasks = variantIds
            .Select(id => DeleteApiResponseAsync($"v1/variants/{id}"))
            .ToList();

        var deleteResponses = await Task.WhenAll(deleteTasks);

        // Assert
        deleteResponses.Should().AllSatisfy(response => response.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task DeleteProductVariant_SequentialDeletes_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple variants
        var variantIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var (_, variantId, _) = await CreateProductWithVariantAsync();
            variantIds.Add(variantId);
        }

        // Act & Assert - Delete variants sequentially
        foreach (var variantId in variantIds)
        {
            var response = await DeleteApiResponseAsync($"v1/variants/{variantId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
            AssertApiSuccess(apiResponse);
        }
    }

    [Fact]
    public async Task DeleteProductVariant_MultipleVariantsFromSameProduct_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product first
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var attributeRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: $"variant_attr_{uniqueId}",
            displayName: $"Variant Attr {uniqueId}",
            type: "Select",
            isVariant: true);
        var attributeResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);
        AssertApiSuccess(attributeResponse);
        var attributeId = attributeResponse!.Data.Id;

        var productRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: $"Multi Variant Product {uniqueId}",
            slug: $"multi-variant-product-{uniqueId}");
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);
        var productId = productResponse!.Data.Id;

        // Add multiple variants
        var variantIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var variantRequest = new
            {
                Sku = $"MULTI-VAR-{i}-{uniqueId}",
                Price = 50.00m + (i * 10),
                StockQuantity = 10,
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = $"Size {i}"
                    }
                },
                IsActive = true
            };

            var variantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>(
                $"v1/products/{productId}/variants", variantRequest);
            AssertApiSuccess(variantResponse);
            variantIds.Add(variantResponse!.Data.Id);
        }

        // Act & Assert - Delete all variants
        foreach (var variantId in variantIds)
        {
            var response = await DeleteApiResponseAsync($"v1/variants/{variantId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
            AssertApiSuccess(apiResponse);
        }
    }

    #endregion

    #region Integration with Other Endpoints Tests

    [Fact]
    public async Task DeleteProductVariant_ThenAttemptToGetIt_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, _) = await CreateProductWithVariantAsync();

        // Verify variant can be retrieved before deletion
        var getResponse = await Client.GetAsync($"v1/variants/{variantId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify variant exists in database before deletion
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId);
            variant.Should().NotBeNull("Variant should exist before deletion");
        });

        // Act - Delete the variant
        var deleteResponse = await DeleteApiResponseAsync($"v1/variants/{variantId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
        var deleteApiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(deleteContent, JsonOptions);
        AssertApiSuccess(deleteApiResponse);

        // Verify variant no longer exists in database after deletion
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId);
            variant.Should().BeNull("Variant should not exist after deletion");
        });

        // Process outbox messages to trigger cache invalidation event handlers
        await ProcessOutboxMessagesAsync();

        // Assert - Verify variant can no longer be retrieved via API
        var getAfterDeleteResponse = await Client.GetAsync($"v1/variants/{variantId}");
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProductVariant_ThenAttemptToUpdateIt_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, _) = await CreateProductWithVariantAsync();

        // Delete the variant
        var deleteResponse = await DeleteApiResponseAsync($"v1/variants/{variantId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
        var deleteApiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(deleteContent, JsonOptions);
        AssertApiSuccess(deleteApiResponse);

        // Process outbox messages
        await ProcessOutboxMessagesAsync();

        // Act - Try to update the deleted variant using multipart
        var updateRequest = new
        {
            Price = 199.99m,
            StockQuantity = 50
        };
        var updateResponse = await PutMultipartAsync($"v1/variants/{variantId}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProductVariant_ProductShouldStillExist()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, _) = await CreateProductWithVariantAsync();

        // Verify product exists before variant deletion
        var getProductBeforeResponse = await Client.GetAsync($"v1/products/{productId}");
        getProductBeforeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Delete the variant
        var deleteResponse = await DeleteApiResponseAsync($"v1/variants/{variantId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
        var deleteApiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(deleteContent, JsonOptions);
        AssertApiSuccess(deleteApiResponse);

        // Assert - Product should still exist
        var getProductAfterResponse = await Client.GetAsync($"v1/products/{productId}");
        getProductAfterResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);
            product.Should().NotBeNull("Product should still exist after variant deletion");

            var variant = await context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId);
            variant.Should().BeNull("Variant should not exist after deletion");
        });
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task DeleteProductVariant_SuccessResponse_ShouldHaveCorrectFormat()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, _) = await CreateProductWithVariantAsync();

        // Act
        var response = await DeleteApiResponseAsync($"v1/variants/{variantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
        apiResponse.Should().NotBeNull();
        apiResponse!.Succeeded.Should().BeTrue();
        apiResponse.Data.Should().NotBeNullOrEmpty();
        apiResponse.Data.Should().Be("Product variant deleted successfully");
        apiResponse.Message.Should().NotBeNullOrEmpty();
        apiResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task DeleteProductVariant_ErrorResponse_ShouldHaveCorrectFormat()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/variants/{nonExistentId}");

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

    #region Variant Attributes Tests

    [Fact]
    public async Task DeleteProductVariant_WithMultipleAttributes_ShouldDeleteAllAttributes()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple attributes
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var attributeIds = new List<Guid>();

        for (int i = 0; i < 3; i++)
        {
            var attributeRequest = AttributeTestDataV1.Creation.CreateValidRequest(
                name: $"multi_attr_{i}_{uniqueId}",
                displayName: $"Multi Attr {i}",
                type: "Select",
                isVariant: true);
            var attributeResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);
            AssertApiSuccess(attributeResponse);
            attributeIds.Add(attributeResponse!.Data.Id);
        }

        // Create product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: $"Multi Attr Product {uniqueId}",
            slug: $"multi-attr-product-{uniqueId}");
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);
        var productId = productResponse!.Data.Id;

        // Add variant with multiple attributes
        var variantRequest = new
        {
            Sku = $"MULTI-ATTR-VAR-{uniqueId}",
            Price = 99.99m,
            StockQuantity = 10,
            Attributes = attributeIds.Select((id, index) => new
            {
                AttributeId = id,
                Value = $"Value {index}"
            }).ToArray(),
            IsActive = true
        };

        var variantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>(
            $"v1/products/{productId}/variants", variantRequest);
        AssertApiSuccess(variantResponse);
        var variantId = variantResponse!.Data.Id;

        // Verify variant attributes exist
        await ExecuteDbContextAsync(async context =>
        {
            var variantAttributes = await context.VariantAttributes
                .Where(va => va.VariantId == variantId)
                .ToListAsync();
            variantAttributes.Should().HaveCount(3);
        });

        // Act - Delete the variant
        var deleteResponse = await DeleteApiResponseAsync($"v1/variants/{variantId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify variant attributes are also deleted
        await ExecuteDbContextAsync(async context =>
        {
            var variantAttributes = await context.VariantAttributes
                .Where(va => va.VariantId == variantId)
                .ToListAsync();
            variantAttributes.Should().BeEmpty("All variant attributes should be deleted with the variant");
        });
    }

    #endregion
}
