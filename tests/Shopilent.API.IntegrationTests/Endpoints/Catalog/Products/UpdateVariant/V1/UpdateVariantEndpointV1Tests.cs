using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;
using Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;
using Shopilent.API.Endpoints.Catalog.Products.CreateProduct.V1;
using Shopilent.Application.Features.Catalog.Commands.AddProductVariant.V1;
using Shopilent.Application.Features.Catalog.Commands.UpdateVariant.V1;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Products.UpdateVariant.V1;

public class UpdateVariantEndpointV1Tests : ApiIntegrationTestBase
{
    public UpdateVariantEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Helper Methods

    private async Task<(Guid ProductId, Guid VariantId, Guid AttributeId, string OriginalSku)> CreateProductWithVariantAsync()
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

        // Create a product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: $"Test Product {uniqueId}",
            slug: $"test-product-{uniqueId}");
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);
        var productId = productResponse!.Data.Id;

        // Create a variant
        var originalSku = $"ORIG-SKU-{uniqueId}";
        var variantRequest = new
        {
            Sku = originalSku,
            Price = 99.99m,
            StockQuantity = 10,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Large"
                }
            },
            IsActive = true
        };

        var variantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>($"v1/products/{productId}/variants", variantRequest);
        AssertApiSuccess(variantResponse);
        var variantId = variantResponse!.Data.Id;

        return (productId, variantId, attributeId, originalSku);
    }

    private async Task<ApiResponse<UpdateVariantResponseV1>?> PutUpdateVariantAsync(Guid variantId, object request)
    {
        return await PutMultipartApiResponseAsync<UpdateVariantResponseV1>($"v1/variants/{variantId}", request);
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task UpdateVariant_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, attributeId, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Sku = "UPDATED-SKU-001",
            Price = 149.99m,
            StockQuantity = 20,
            Metadata = new Dictionary<string, object>
            {
                { "updated_field", "updated_value" },
                { "color", "Blue" }
            },
            IsActive = true
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(variantId);
        response.Data.ProductId.Should().Be(productId);
        response.Data.Sku.Should().Be("UPDATED-SKU-001");
        response.Data.Price.Should().Be(149.99m);
        response.Data.StockQuantity.Should().Be(20);
        response.Data.IsActive.Should().BeTrue();
        response.Data.Metadata.Should().ContainKey("updated_field");
        response.Data.Metadata.Should().ContainKey("color");
        response.Data.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateVariant_WithValidData_ShouldUpdateInDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Sku = "DB-UPDATED-SKU",
            Price = 199.99m,
            StockQuantity = 50,
            IsActive = false
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId);

            variant.Should().NotBeNull();
            variant!.ProductId.Should().Be(productId);
            variant.Sku.Should().Be("DB-UPDATED-SKU");
            variant.Price.Amount.Should().Be(199.99m);
            variant.StockQuantity.Should().Be(50);
            variant.IsActive.Should().BeFalse();
            variant.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        });
    }

    [Fact]
    public async Task UpdateVariant_WithPartialData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, originalSku) = await CreateProductWithVariantAsync();

        // Only update price and stock
        var updateRequest = new
        {
            Price = 79.99m,
            StockQuantity = 5
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Price.Should().Be(79.99m);
        response.Data.StockQuantity.Should().Be(5);
        response.Data.Sku.Should().Be(originalSku); // SKU should remain unchanged
    }

    [Fact]
    public async Task UpdateVariant_WithOnlySku_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Sku = "SKU-ONLY-UPDATE"
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be("SKU-ONLY-UPDATE");
        response.Data.Price.Should().Be(99.99m); // Original price
        response.Data.StockQuantity.Should().Be(10); // Original stock
    }

    [Fact]
    public async Task UpdateVariant_WithOnlyPrice_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, originalSku) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Price = 249.99m
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Price.Should().Be(249.99m);
        response.Data.Sku.Should().Be(originalSku); // Original SKU
    }

    [Fact]
    public async Task UpdateVariant_WithOnlyStockQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            StockQuantity = 100
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(100);
    }

    [Fact]
    public async Task UpdateVariant_WithMetadata_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Metadata = new Dictionary<string, object>
            {
                { "brand", "Premium Brand" },
                { "warranty_years", 5 },
                { "eco_friendly", true },
                { "certifications", new[] { "ISO9001", "CE" } }
            }
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Metadata.Should().NotBeEmpty();
        response.Data.Metadata.Should().ContainKey("brand");
        response.Data.Metadata.Should().ContainKey("warranty_years");
        response.Data.Metadata.Should().ContainKey("eco_friendly");
        response.Data.Metadata.Should().ContainKey("certifications");
    }

    [Fact]
    public async Task UpdateVariant_WithActivation_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        // First deactivate
        var deactivateRequest = new { IsActive = false };
        var deactivateResponse = await PutUpdateVariantAsync(variantId, deactivateRequest);
        AssertApiSuccess(deactivateResponse);

        // Then activate
        var activateRequest = new { IsActive = true };

        // Act
        var response = await PutUpdateVariantAsync(variantId, activateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateVariant_WithDeactivation_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new { IsActive = false };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateVariant_WithManagerRole_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        // Re-authenticate as manager
        var managerToken = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(managerToken);

        var updateRequest = new
        {
            Sku = "MGR-UPDATED-SKU",
            Price = 129.99m
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be("MGR-UPDATED-SKU");
    }

    [Fact]
    public async Task UpdateVariant_WithZeroPrice_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new { Price = 0m };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Price.Should().Be(0m);
    }

    [Fact]
    public async Task UpdateVariant_WithZeroStockQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new { StockQuantity = 0 };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(0);
    }

    [Fact]
    public async Task UpdateVariant_WithEmptyMetadata_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Metadata = new Dictionary<string, object>()
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task UpdateVariant_KeepingSameSku_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, originalSku) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Sku = originalSku, // Same SKU
            Price = 199.99m
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be(originalSku);
        response.Data.Price.Should().Be(199.99m);
    }

    #endregion

    #region Validation Tests - SKU

    [Fact]
    public async Task UpdateVariant_WithExcessiveSkuLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Sku = new string('S', 101) // Exceeds 100 characters
        };

        // Act
        var response = await PutMultipartAsync($"v1/variants/{variantId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("100 characters");
        content.Should().Contain("SKU");
    }

    [Fact]
    public async Task UpdateVariant_WithMaximumSkuLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Sku = new string('S', 100) // Exactly 100 characters
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Length.Should().Be(100);
    }

    [Fact]
    public async Task UpdateVariant_WithDuplicateSku_ShouldReturnConflict()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, variantId1, attributeId, _) = await CreateProductWithVariantAsync();

        // Create second variant with different SKU
        var variant2Request = new
        {
            Sku = "VARIANT-2-SKU",
            Price = 89.99m,
            StockQuantity = 5,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Medium"
                }
            }
        };

        var variant2Response = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>($"v1/products/{productId}/variants", variant2Request);
        AssertApiSuccess(variant2Response);

        // Try to update variant1 with variant2's SKU
        var updateRequest = new
        {
            Sku = "VARIANT-2-SKU" // Duplicate SKU
        };

        // Act
        var response = await PutMultipartAsync($"v1/variants/{variantId1}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().ContainAny("duplicate", "already exists", "SKU");
    }

    [Fact]
    public async Task UpdateVariant_WithSpecialCharactersInSku_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Sku = "VAR-SKU_2024-UPDATE.001"
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be("VAR-SKU_2024-UPDATE.001");
    }

    #endregion

    #region Validation Tests - Price

    [Fact]
    public async Task UpdateVariant_WithNegativePrice_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Price = -10.50m
        };

        // Act
        var response = await PutMultipartAsync($"v1/variants/{variantId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("greater than or equal to zero");
    }

    #endregion

    #region Validation Tests - Stock Quantity

    [Fact]
    public async Task UpdateVariant_WithNegativeStockQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            StockQuantity = -5
        };

        // Act
        var response = await PutMultipartAsync($"v1/variants/{variantId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("greater than or equal to zero");
    }

    [Fact]
    public async Task UpdateVariant_WithMaximumStockQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            StockQuantity = int.MaxValue
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(int.MaxValue);
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task UpdateVariant_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();

        var variantId = Guid.NewGuid();
        var updateRequest = new
        {
            Sku = "UNAUTH-SKU",
            Price = 99.99m
        };

        // Act
        var response = await PutMultipartAsync($"v1/variants/{variantId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateVariant_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        // Re-authenticate as customer
        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);

        var updateRequest = new
        {
            Sku = "CUSTOMER-SKU",
            Price = 99.99m
        };

        // Act
        var response = await PutMultipartAsync($"v1/variants/{variantId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task UpdateVariant_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var nonExistentId = Guid.NewGuid();
        var updateRequest = new
        {
            Sku = "NON-EXISTENT-SKU",
            Price = 99.99m
        };

        // Act
        var response = await PutMultipartAsync($"v1/variants/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().ContainAny("variant", "not found");
    }

    [Fact]
    public async Task UpdateVariant_WithInvalidGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var updateRequest = new
        {
            Sku = "INVALID-GUID-SKU",
            Price = 99.99m
        };

        // Act
        var response = await PutMultipartAsync("v1/variants/invalid-guid", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task UpdateVariant_WithMinimumValidSku_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Sku = "A" // Single character
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be("A");
    }

    [Fact]
    public async Task UpdateVariant_WithVeryLargePrice_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Price = 999999.99m
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Price.Should().Be(999999.99m);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task UpdateVariant_WithUnicodeInSku_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Sku = "SKU-Café-München-™"
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be("SKU-Café-München-™");
    }

    [Fact]
    public async Task UpdateVariant_WithComplexMetadata_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Metadata = new Dictionary<string, object>
            {
                { "manufacturer", "Acme Corp" },
                { "warranty_months", 24 },
                { "imported", true },
                { "tags", new[] { "premium", "limited-edition" } },
                { "specifications", new { weight = 1.5, dimensions = "10x20x30" } }
            }
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Metadata.Should().NotBeEmpty();
        response.Data.Metadata.Should().ContainKey("manufacturer");
        response.Data.Metadata.Should().ContainKey("warranty_months");
        response.Data.Metadata.Should().ContainKey("imported");
        response.Data.Metadata.Should().ContainKey("tags");
        response.Data.Metadata.Should().ContainKey("specifications");
    }

    [Fact]
    public async Task UpdateVariant_MultipleSequentialUpdates_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        // First update
        var update1 = new { Price = 120m };
        var response1 = await PutUpdateVariantAsync(variantId, update1);
        AssertApiSuccess(response1);

        // Second update
        var update2 = new { StockQuantity = 50 };
        var response2 = await PutUpdateVariantAsync(variantId, update2);
        AssertApiSuccess(response2);

        // Third update
        var update3 = new { Sku = "FINAL-SKU" };
        var response3 = await PutUpdateVariantAsync(variantId, update3);

        // Assert
        AssertApiSuccess(response3);
        response3!.Data.Sku.Should().Be("FINAL-SKU");
        response3.Data.Price.Should().Be(120m); // From first update
        response3.Data.StockQuantity.Should().Be(50); // From second update
    }

    [Fact]
    public async Task UpdateVariant_WithAllFieldsUpdated_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            Sku = "ALL-FIELDS-SKU",
            Price = 299.99m,
            StockQuantity = 75,
            IsActive = false,
            Metadata = new Dictionary<string, object>
            {
                { "all_updated", true }
            }
        };

        // Act
        var response = await PutUpdateVariantAsync(variantId, updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be("ALL-FIELDS-SKU");
        response.Data.Price.Should().Be(299.99m);
        response.Data.StockQuantity.Should().Be(75);
        response.Data.IsActive.Should().BeFalse();
        response.Data.Metadata.Should().ContainKey("all_updated");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task UpdateVariant_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple variants
        var variantIds = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var (_, variantId, _, _) = await CreateProductWithVariantAsync();
            variantIds.Add(variantId);
        }

        // Update all concurrently
        var tasks = variantIds.Select((id, index) => new
        {
            Sku = $"CONCURRENT-UPDATE-{index}-{Guid.NewGuid():N}",
            Price = 100m + index * 10,
            StockQuantity = 10 + index
        })
        .Select((request, index) => PutUpdateVariantAsync(variantIds[index], request))
        .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Select(r => r!.Data.Id).Should().OnlyHaveUniqueItems();
        responses.Select(r => r!.Data.Sku).Should().OnlyHaveUniqueItems();
        responses.Should().AllSatisfy(response =>
            response!.Data.Sku.Should().StartWith("CONCURRENT-UPDATE-"));
    }

    #endregion

    #region Image Management Validation Tests

    [Fact]
    public async Task UpdateVariant_WithBothRemoveExistingImagesAndImagesToRemove_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            RemoveExistingImages = true,
            ImagesToRemove = new List<string> { "image-key-1" }
        };

        // Act
        var response = await PutMultipartAsync($"v1/variants/{variantId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Cannot specify both RemoveExistingImages and ImagesToRemove");
    }

    [Fact]
    public async Task UpdateVariant_WithDuplicateDisplayOrders_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            ImageOrders = new[]
            {
                new { ImageKey = "image-1", DisplayOrder = 1, IsDefault = false },
                new { ImageKey = "image-2", DisplayOrder = 1, IsDefault = false } // Duplicate order
            }
        };

        // Act
        var response = await PutMultipartAsync($"v1/variants/{variantId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Duplicate display orders are not allowed");
    }

    [Fact]
    public async Task UpdateVariant_WithDuplicateImageKeys_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            ImageOrders = new[]
            {
                new { ImageKey = "image-1", DisplayOrder = 1, IsDefault = false },
                new { ImageKey = "image-1", DisplayOrder = 2, IsDefault = false } // Duplicate key
            }
        };

        // Act
        var response = await PutMultipartAsync($"v1/variants/{variantId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Duplicate image keys are not allowed");
    }

    [Fact]
    public async Task UpdateVariant_WithNegativeDisplayOrder_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            ImageOrders = new[]
            {
                new { ImageKey = "image-1", DisplayOrder = -1, IsDefault = false }
            }
        };

        // Act
        var response = await PutMultipartAsync($"v1/variants/{variantId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Display order must be greater than or equal to zero");
    }

    [Fact]
    public async Task UpdateVariant_WithEmptyImageKey_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, variantId, _, _) = await CreateProductWithVariantAsync();

        var updateRequest = new
        {
            ImagesToRemove = new List<string> { "" } // Empty image key
        };

        // Act
        var response = await PutMultipartAsync($"v1/variants/{variantId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Image key cannot be empty");
    }

    #endregion
}
