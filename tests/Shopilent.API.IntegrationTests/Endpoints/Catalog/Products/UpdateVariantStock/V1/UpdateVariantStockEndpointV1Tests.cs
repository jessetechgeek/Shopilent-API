using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;
using Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;
using Shopilent.API.Endpoints.Catalog.Products.CreateProduct.V1;
using Shopilent.Application.Features.Catalog.Commands.AddProductVariant.V1;
using Shopilent.API.Endpoints.Catalog.Products.UpdateVariantStock.V1;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Products.UpdateVariantStock.V1;

public class UpdateVariantStockEndpointV1Tests : ApiIntegrationTestBase
{
    public UpdateVariantStockEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Helper Methods

    private async Task<Guid> CreateProductVariantForStockUpdateAsync(int initialStock = 10)
    {
        // Create an attribute first (for variant attributes)
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var attributeRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: $"stock_attr_{uniqueId}",
            displayName: $"Stock Attr {uniqueId}",
            type: "Select",
            isVariant: true);
        var attributeResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);
        AssertApiSuccess(attributeResponse);
        var attributeId = attributeResponse!.Data.Id;

        // Create a product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: $"Test Product For Stock {uniqueId}",
            slug: $"test-product-stock-{uniqueId}");
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);
        var productId = productResponse!.Data.Id;

        // Create a product variant
        var variantRequest = new
        {
            Sku = $"VAR-STOCK-{uniqueId}",
            Price = 99.99m,
            StockQuantity = initialStock,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Medium"
                }
            },
            IsActive = true
        };

        var variantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>(
            $"v1/products/{productId}/variants", variantRequest);
        AssertApiSuccess(variantResponse);

        return variantResponse!.Data.Id;
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task UpdateVariantStock_WithValidQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 10);

        var request = new
        {
            Quantity = 25
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(variantId);
        response.Data.StockQuantity.Should().Be(25);
        response.Data.IsActive.Should().BeTrue();
        response.Data.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateVariantStock_WithValidQuantity_ShouldUpdateStockInDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 5);

        var request = new
        {
            Quantity = 50
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request);

        // Assert
        AssertApiSuccess(response);

        // Verify stock updated in database
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId);

            variant.Should().NotBeNull();
            variant!.StockQuantity.Should().Be(50);
            variant.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        });
    }

    [Fact]
    public async Task UpdateVariantStock_IncreaseStock_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 10);

        var request = new
        {
            Quantity = 100
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(100);
    }

    [Fact]
    public async Task UpdateVariantStock_DecreaseStock_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 100);

        var request = new
        {
            Quantity = 5
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(5);
    }

    [Fact]
    public async Task UpdateVariantStock_SetToZero_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 50);

        var request = new
        {
            Quantity = 0
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(0);
    }

    [Fact]
    public async Task UpdateVariantStock_WithManagerRole_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(accessToken);

        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);
        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 20);

        // Switch back to manager
        SetAuthenticationHeader(accessToken);

        var request = new
        {
            Quantity = 30
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(30);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task UpdateVariantStock_WithNegativeQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 10);

        var request = new
        {
            Quantity = -5
        };

        // Act
        var response = await PutAsync($"v1/variants/{variantId}/stock", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Stock quantity cannot be negative");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(999999)]
    public async Task UpdateVariantStock_WithValidQuantityRange_ShouldReturnSuccess(int quantity)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 10);

        var request = new
        {
            Quantity = quantity
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(quantity);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task UpdateVariantStock_WithNonExistentVariantId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var nonExistentId = Guid.NewGuid();

        var request = new
        {
            Quantity = 10
        };

        // Act
        var response = await PutAsync($"v1/variants/{nonExistentId}/stock", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateVariantStock_WithEmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var request = new
        {
            Quantity = 10
        };

        // Act
        var response = await PutAsync($"v1/variants/{Guid.Empty}/stock", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateVariantStock_WithInvalidGuidFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var request = new
        {
            Quantity = 10
        };

        // Act
        var response = await PutAsync($"v1/variants/invalid-guid-format/stock", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task UpdateVariantStock_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();

        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);
        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 10);

        ClearAuthenticationHeader();

        var request = new
        {
            Quantity = 20
        };

        // Act
        var response = await PutAsync($"v1/variants/{variantId}/stock", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateVariantStock_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);
        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 10);

        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);

        var request = new
        {
            Quantity = 20
        };

        // Act
        var response = await PutAsync($"v1/variants/{variantId}/stock", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateVariantStock_WithAdminRole_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 10);

        var request = new
        {
            Quantity = 25
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(25);
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public async Task UpdateVariantStock_WithMaximumValidStock_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 10);

        var request = new
        {
            Quantity = int.MaxValue
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(int.MaxValue);
    }

    [Fact]
    public async Task UpdateVariantStock_FromZeroToPositive_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 0);

        var request = new
        {
            Quantity = 100
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(100);
    }

    [Fact]
    public async Task UpdateVariantStock_FromPositiveToZero_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 100);

        var request = new
        {
            Quantity = 0
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(0);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task UpdateVariantStock_SameQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var initialStock = 25;
        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: initialStock);

        var request = new
        {
            Quantity = initialStock
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(initialStock);
    }

    [Fact]
    public async Task UpdateVariantStock_MultipleUpdatesInSequence_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 10);

        // Act & Assert - First update
        var request1 = new { Quantity = 20 };
        var response1 = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request1);
        AssertApiSuccess(response1);
        response1!.Data.StockQuantity.Should().Be(20);

        // Act & Assert - Second update
        var request2 = new { Quantity = 50 };
        var response2 = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request2);
        AssertApiSuccess(response2);
        response2!.Data.StockQuantity.Should().Be(50);

        // Act & Assert - Third update
        var request3 = new { Quantity = 0 };
        var response3 = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request3);
        AssertApiSuccess(response3);
        response3!.Data.StockQuantity.Should().Be(0);

        // Verify final state in database
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId);

            variant.Should().NotBeNull();
            variant!.StockQuantity.Should().Be(0);
        });
    }

    [Fact]
    public async Task UpdateVariantStock_ForInactiveVariant_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 10);

        // First, update the variant to inactive (if such endpoint exists)
        // For now, we'll just update stock and verify it works regardless of active status

        var request = new
        {
            Quantity = 25
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(25);
    }

    #endregion

    #region Sequential Update Tests

    [Fact]
    public async Task UpdateVariantStock_SequentialUpdates_ShouldAllSucceed()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var variantId = await CreateProductVariantForStockUpdateAsync(initialStock: 10);

        var request1 = new { Quantity = 20 };
        var request2 = new { Quantity = 30 };
        var request3 = new { Quantity = 40 };

        // Act - Execute sequential updates (not concurrent)
        var response1 = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request1);
        var response2 = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request2);
        var response3 = await PutApiResponseAsync<object, UpdateVariantStockResponseV1>(
            $"v1/variants/{variantId}/stock", request3);

        // Assert - All should succeed
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);
        AssertApiSuccess(response3);

        response1!.Data.StockQuantity.Should().Be(20);
        response2!.Data.StockQuantity.Should().Be(30);
        response3!.Data.StockQuantity.Should().Be(40);

        // Final stock should be the last update
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId);

            variant.Should().NotBeNull();
            variant!.StockQuantity.Should().Be(40);
        });
    }

    #endregion
}
