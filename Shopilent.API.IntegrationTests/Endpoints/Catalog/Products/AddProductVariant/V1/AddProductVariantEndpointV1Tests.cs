using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;
using Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;
using Shopilent.API.Endpoints.Catalog.Products.CreateProduct.V1;
using Shopilent.Application.Features.Catalog.Commands.AddProductVariant.V1;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Products.AddProductVariant.V1;

public class AddProductVariantEndpointV1Tests : ApiIntegrationTestBase
{
    public AddProductVariantEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Helper Methods

    private async Task<(Guid ProductId, Guid AttributeId)> CreateProductAndAttributeForVariantAsync()
    {
        // Create an attribute first (for variant attributes)
        // Use unique names to avoid conflicts
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var attributeRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: $"variant_attr_{uniqueId}",
            displayName: $"Variant Attr {uniqueId}",
            type: "Select",
            isVariant: true);
        var attributeResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);
        AssertApiSuccess(attributeResponse);
        var attributeId = attributeResponse!.Data.Id;

        // Create a product with unique slug to avoid conflicts
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: $"Test Product For Variants {uniqueId}",
            slug: $"test-product-variants-{uniqueId}");
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);
        var productId = productResponse!.Data.Id;

        return (productId, attributeId);
    }

    private async Task<ApiResponse<AddProductVariantResponseV1>?> PostAddVariantAsync(Guid productId, object request)
    {
        // Use PostMultipartApiResponseAsync since the endpoint accepts multipart/form-data
        return await PostMultipartApiResponseAsync<AddProductVariantResponseV1>($"v1/products/{productId}/variants", request);
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task AddProductVariant_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "VAR-SKU-001",
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
            IsActive = true,
            Metadata = new Dictionary<string, object>
            {
                { "color", "Red" },
                { "material", "Cotton" }
            }
        };

        // Act
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().NotBeEmpty();
        response.Data.ProductId.Should().Be(productId);
        response.Data.Sku.Should().Be("VAR-SKU-001");
        response.Data.Price.Should().Be(99.99m);
        response.Data.StockQuantity.Should().Be(10);
        response.Data.IsActive.Should().BeTrue();
        response.Data.Attributes.Should().ContainSingle();
        response.Data.Attributes.First().AttributeId.Should().Be(attributeId);
        response.Data.Attributes.First().Value.ToString().Should().Be("Large");
        response.Data.Metadata.Should().ContainKey("color");
        response.Data.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddProductVariant_WithValidData_ShouldCreateVariantInDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "VAR-DB-TEST-001",
            Price = 49.99m,
            StockQuantity = 5,
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

        // Act
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);

        // Verify variant exists in database
        await ExecuteDbContextAsync(async context =>
        {
            var variant = await context.ProductVariants
                .Include(v => v.VariantAttributes)
                .FirstOrDefaultAsync(v => v.Id == response!.Data.Id);

            variant.Should().NotBeNull();
            variant!.ProductId.Should().Be(productId);
            variant.Sku.Should().Be("VAR-DB-TEST-001");
            variant.Price.Amount.Should().Be(49.99m);
            variant.StockQuantity.Should().Be(5);
            variant.IsActive.Should().BeTrue();
            variant.VariantAttributes.Should().ContainSingle();
            variant.VariantAttributes.First().AttributeId.Should().Be(attributeId);
        });
    }

    [Fact]
    public async Task AddProductVariant_WithMinimalData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "VAR-DB-TEST-001",
            Price = 49.99m,
            StockQuantity = 0,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Small"
                }
            },
            IsActive = true
        };

        // Act
        var response = await PostAddVariantAsync(productId, request);

        response.Should().NotBeNull("Response should not be null");

        AssertApiSuccess(response);
        response.Data.Sku.Should().Be("VAR-DB-TEST-001");
        response.Data.Price.Should().Be(49.99m);
        response.Data.StockQuantity.Should().Be(0);
    }

    [Fact]
    public async Task AddProductVariant_WithNullPrice_ShouldInheritProductBasePrice()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "INHERIT-PRICE-SKU",
            Price = (decimal?)null, // NULL price should inherit from product
            StockQuantity = 10,
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

        // Act
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().NotBeEmpty();
        response.Data.Sku.Should().Be("INHERIT-PRICE-SKU");

        // Variant should have inherited the product's base price
        // ProductTestDataV1 creates products with random prices between 10-500
        response.Data.Price.Should().BeGreaterThan(0);
        response.Data.StockQuantity.Should().Be(10);
    }

    [Fact]
    public async Task AddProductVariant_WithManagerRole_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        // Re-authenticate as manager
        var managerToken = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(managerToken);

        var request = new
        {
            Sku = "MGR-VAR-001",
            Price = 79.99m,
            StockQuantity = 15,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "XL"
                }
            }
        };

        // Act
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be("MGR-VAR-001");
    }

    [Fact]
    public async Task AddProductVariant_WithInactiveStatus_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "INACTIVE-VAR-001",
            Price = 59.99m,
            StockQuantity = 0,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "XXL"
                }
            },
            IsActive = false
        };

        // Act
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task AddProductVariant_WithMultipleAttributes_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId1) = await CreateProductAndAttributeForVariantAsync();

        // Create second variant attribute
        var attribute2Request = AttributeTestDataV1.Creation.CreateValidRequest(
            name: "variant_color",
            displayName: "Color",
            type: "Color",
            isVariant: true);
        var attribute2Response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attribute2Request);
        AssertApiSuccess(attribute2Response);
        var attributeId2 = attribute2Response!.Data.Id;

        var request = new
        {
            Sku = "MULTI-ATTR-VAR-001",
            Price = 89.99m,
            StockQuantity = 20,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId1,
                    Value = "Large"
                },
                new
                {
                    AttributeId = attributeId2,
                    Value = "Blue"
                }
            }
        };

        // Act
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Attributes.Should().HaveCount(2);
        response.Data.Attributes.Should().Contain(a => a.AttributeId == attributeId1);
        response.Data.Attributes.Should().Contain(a => a.AttributeId == attributeId2);
    }

    [Fact]
    public async Task AddProductVariant_WithComplexMetadata_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "META-VAR-001",
            Price = 119.99m,
            StockQuantity = 8,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "XS"
                }
            },
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
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Metadata.Should().NotBeEmpty();
        response.Data.Metadata.Should().ContainKey("manufacturer");
        response.Data.Metadata.Should().ContainKey("warranty_months");
    }

    #endregion

    #region Validation Tests - Attributes

    [Fact]
    public async Task AddProductVariant_WithoutAttributes_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, _) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "NO-ATTR-VAR-001",
            Price = 99.99m,
            StockQuantity = 10,
            Attributes = Array.Empty<object>()
        };

        // Act
        var response = await PostMultipartAsync($"v1/products/{productId}/variants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("At least one attribute value is required");
    }

    [Fact]
    public async Task AddProductVariant_WithInvalidAttributeId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, _) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "INVALID-ATTR-VAR-001",
            Price = 99.99m,
            StockQuantity = 10,
            Attributes = new[]
            {
                new
                {
                    AttributeId = Guid.NewGuid(), // Non-existent attribute
                    Value = "Large"
                }
            }
        };

        // Act
        var response = await PostMultipartAsync($"v1/products/{productId}/variants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().ContainAny("Attribute", "not found");
    }

    [Fact]
    public async Task AddProductVariant_WithNonVariantAttribute_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, _) = await CreateProductAndAttributeForVariantAsync();

        // Create a non-variant attribute
        var nonVariantAttrRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: "non_variant_attr",
            displayName: "Non-Variant Attribute",
            type: "Text",
            isVariant: false);
        var nonVariantAttrResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", nonVariantAttrRequest);
        AssertApiSuccess(nonVariantAttrResponse);
        var nonVariantAttrId = nonVariantAttrResponse!.Data.Id;

        var request = new
        {
            Sku = "NON-VAR-ATTR-001",
            Price = 99.99m,
            StockQuantity = 10,
            Attributes = new[]
            {
                new
                {
                    AttributeId = nonVariantAttrId,
                    Value = "Test Value"
                }
            }
        };

        // Act
        var response = await PostMultipartAsync($"v1/products/{productId}/variants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().ContainAny("variant attribute", "not a variant");
    }

    #endregion

    #region Validation Tests - SKU

    [Fact]
    public async Task AddProductVariant_WithExcessiveSkuLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = new string('S', 101), // Exceeds 100 character limit
            Price = 99.99m,
            StockQuantity = 10,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Large"
                }
            }
        };

        // Act
        var response = await PostMultipartAsync($"v1/products/{productId}/variants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("100 characters");
        content.Should().Contain("SKU");
    }

    [Fact]
    public async Task AddProductVariant_WithDuplicateSku_ShouldReturnConflict()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var sku = "DUPLICATE-VAR-SKU-001";
        var firstRequest = new
        {
            Sku = sku,
            Price = 99.99m,
            StockQuantity = 10,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Large"
                }
            }
        };

        // Act - Create first variant
        var firstResponse = await PostAddVariantAsync(productId, firstRequest);
        AssertApiSuccess(firstResponse);

        // Create second variant with same SKU
        var secondRequest = new
        {
            Sku = sku,
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

        var secondResponse = await PostMultipartAsync($"v1/products/{productId}/variants", secondRequest);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await secondResponse.Content.ReadAsStringAsync();
        content.Should().ContainAny("duplicate", "already exists", "SKU");
    }

    #endregion

    #region Validation Tests - Price

    [Fact]
    public async Task AddProductVariant_WithNegativePrice_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "NEG-PRICE-VAR-001",
            Price = -10.50m,
            StockQuantity = 10,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Large"
                }
            }
        };

        // Act
        var response = await PostMultipartAsync($"v1/products/{productId}/variants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("non-negative");
    }

    [Fact]
    public async Task AddProductVariant_WithZeroPrice_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "ZERO-PRICE-VAR-001",
            Price = 0m,
            StockQuantity = 10,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Free Sample"
                }
            }
        };

        // Act
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Price.Should().Be(0m);
    }

    #endregion

    #region Validation Tests - Stock Quantity

    [Fact]
    public async Task AddProductVariant_WithNegativeStockQuantity_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "NEG-STOCK-VAR-001",
            Price = 99.99m,
            StockQuantity = -5,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Large"
                }
            }
        };

        // Act
        var response = await PostMultipartAsync($"v1/products/{productId}/variants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("non-negative");
        content.Should().ContainAny("stock", "quantity");
    }

    [Fact]
    public async Task AddProductVariant_WithZeroStockQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "ZERO-STOCK-VAR-001",
            Price = 99.99m,
            StockQuantity = 0,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Out of Stock"
                }
            }
        };

        // Act
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(0);
    }

    #endregion

    #region Validation Tests - Product

    [Fact]
    public async Task AddProductVariant_WithNonExistentProductId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (_, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var nonExistentProductId = Guid.NewGuid();
        var request = new
        {
            Sku = "NO-PRODUCT-VAR-001",
            Price = 99.99m,
            StockQuantity = 10,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Large"
                }
            }
        };

        // Act
        var response = await PostMultipartAsync($"v1/products/{nonExistentProductId}/variants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().ContainAny("Product", "not found");
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task AddProductVariant_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();

        var productId = Guid.NewGuid();
        var attributeId = Guid.NewGuid();
        var request = new
        {
            Sku = "UNAUTH-VAR-001",
            Price = 99.99m,
            StockQuantity = 10,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Large"
                }
            }
        };

        // Act
        var response = await PostMultipartAsync($"v1/products/{productId}/variants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddProductVariant_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        // Re-authenticate as customer
        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);

        var request = new
        {
            Sku = "CUSTOMER-VAR-001",
            Price = 99.99m,
            StockQuantity = 10,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Large"
                }
            }
        };

        // Act
        var response = await PostMultipartAsync($"v1/products/{productId}/variants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task AddProductVariant_WithMaximumSkuLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = new string('S', 100), // Exactly 100 characters
            Price = 99.99m,
            StockQuantity = 10,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Large"
                }
            }
        };

        // Act
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Length.Should().Be(100);
    }

    [Fact]
    public async Task AddProductVariant_WithMaximumStockQuantity_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "MAX-STOCK-VAR-001",
            Price = 99.99m,
            StockQuantity = int.MaxValue,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Bulk"
                }
            }
        };

        // Act
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(int.MaxValue);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task AddProductVariant_WithEmptyMetadata_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "EMPTY-META-VAR-001",
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
            Metadata = new Dictionary<string, object>()
        };

        // Act
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Metadata.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task AddProductVariant_WithNullMetadata_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "NULL-META-VAR-001",
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
            Metadata = (Dictionary<string, object>?)null
        };

        // Act
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Metadata.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task AddProductVariant_WithUnicodeInAttributeValue_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "UNICODE-VAR-001",
            Price = 99.99m,
            StockQuantity = 10,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Café Münchën Size™ 测试 🛍️"
                }
            }
        };

        // Act
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Attributes.First().Value.ToString().Should().Be("Café Münchën Size™ 测试 🛍️");
    }

    [Fact]
    public async Task AddProductVariant_WithSpecialCharactersInSku_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var request = new
        {
            Sku = "VAR-SKU_2024-TEST.001",
            Price = 99.99m,
            StockQuantity = 10,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Large"
                }
            }
        };

        // Act
        var response = await PostAddVariantAsync(productId, request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be("VAR-SKU_2024-TEST.001");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task AddProductVariant_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var tasks = Enumerable.Range(0, 5)
            .Select(i => new
            {
                Sku = $"CONCURRENT-VAR-{i}-{Guid.NewGuid():N}",
                Price = 99.99m + i,
                StockQuantity = 10 + i,
                Attributes = new[]
                {
                    new
                    {
                        AttributeId = attributeId,
                        Value = $"Size-{i}"
                    }
                }
            })
            .Select(request => PostAddVariantAsync(productId, request))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Select(r => r!.Data.Id).Should().OnlyHaveUniqueItems();
        responses.Select(r => r!.Data.Sku).Should().OnlyHaveUniqueItems();
    }

    #endregion
}
