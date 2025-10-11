using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Endpoints.Catalog.Products.CreateProduct.V1;
using Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;
using Shopilent.Application.Features.Catalog.Commands.AddProductVariant.V1;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Products.GetVariantBySku.V1;

public class GetVariantBySkuEndpointV1Tests : ApiIntegrationTestBase
{
    public GetVariantBySkuEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Helper Methods

    private async Task<(Guid ProductId, Guid AttributeId)> CreateProductAndAttributeForVariantAsync()
    {
        // Create an attribute first (for variant attributes)
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var attributeRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: $"get_variant_sku_attr_{uniqueId}",
            displayName: $"Get Variant SKU Attr {uniqueId}",
            type: "Select",
            isVariant: true);
        var attributeResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);
        AssertApiSuccess(attributeResponse);
        var attributeId = attributeResponse!.Data.Id;

        // Create a product with unique slug
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: $"Test Product For Get Variant By SKU {uniqueId}",
            slug: $"test-product-get-variant-sku-{uniqueId}");
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);
        var productId = productResponse!.Data.Id;

        return (productId, attributeId);
    }

    private async Task<Guid> CreateVariantAsync(Guid productId, Guid attributeId, string sku, decimal price = 99.99m, int stockQuantity = 50, string attributeValue = "Test Value")
    {
        var variantRequest = new
        {
            Sku = sku,
            Price = price,
            StockQuantity = stockQuantity,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = attributeValue
                }
            },
            IsActive = true,
            Metadata = new Dictionary<string, object>()
        };
        var addVariantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>($"v1/products/{productId}/variants", variantRequest);
        AssertApiSuccess(addVariantResponse);
        return addVariantResponse!.Data.Id;
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task GetVariantBySku_WithValidSku_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-TEST-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 109.99m, 50);

        // Clear auth header to test anonymous access
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(variantId);
        response.Data.ProductId.Should().Be(productId);
        response.Data.Sku.Should().Be(uniqueSku);
        response.Data.Price.Should().Be(109.99m);
        response.Data.Currency.Should().Be("USD");
        response.Data.StockQuantity.Should().Be(50);
        response.Data.IsActive.Should().BeTrue();
        response.Data.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        response.Data.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetVariantBySku_WithValidSku_ShouldReturnCompleteVariantDetails()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create attribute for the variant with unique name
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var attributeRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: $"test_size_sku_{uniqueId}",
            displayName: "Size",
            type: "Select",
            isVariant: true);
        var attributeResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);
        AssertApiSuccess(attributeResponse);
        var attributeId = attributeResponse!.Data.Id;

        // Create product with unique slug
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: $"Complete Variant SKU Test Product {uniqueId}",
            slug: $"complete-variant-sku-test-product-{uniqueId}");
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);
        var productId = productResponse!.Data.Id;

        // Add variant with attributes
        var uniqueSku = $"SKU-COMPLETE-{uniqueId.ToUpper()}";
        var variantRequest = new
        {
            Sku = uniqueSku,
            Price = 199.99m,
            StockQuantity = 100,
            Metadata = new Dictionary<string, object>
            {
                { "color", "Blue" },
                { "material", "Cotton" }
            },
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

        var addVariantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>($"v1/products/{productId}/variants", variantRequest);
        AssertApiSuccess(addVariantResponse);
        var variantId = addVariantResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(variantId);
        response.Data.Sku.Should().Be(uniqueSku);
        response.Data.Price.Should().Be(199.99m);
        response.Data.StockQuantity.Should().Be(100);

        // Verify metadata
        response.Data.Metadata.Should().NotBeNull();
        response.Data.Metadata.Should().ContainKey("color");
        response.Data.Metadata["color"]?.ToString().Should().Be("Blue");

        // Verify attributes
        response.Data.Attributes.Should().NotBeNull();
        response.Data.Attributes.Should().HaveCount(1);
        var attribute = response.Data.Attributes.First();
        attribute.AttributeId.Should().Be(attributeId);
        attribute.Value.Should().ContainKey("value");
        attribute.Value["value"]?.ToString().Should().Be("Large");
    }

    [Fact]
    public async Task GetVariantBySku_CreatedVariant_ShouldBeActiveByDefault()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-ACTIVE-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 89.99m, 25);

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetVariantBySku_ShouldInheritCurrencyFromProduct()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-CURRENCY-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 79.99m, 40);

        // Get the product to check its currency
        var productResponse = await GetApiResponseAsync<ProductDetailDto>($"v1/products/{productId}");
        AssertApiSuccess(productResponse);
        var expectedCurrency = productResponse!.Data.Currency;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Currency.Should().Be(expectedCurrency, "Variant should inherit currency from its parent product");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetVariantBySku_WithNonExistentSku_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentSku = $"NON-EXISTENT-SKU-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        // Act
        var response = await Client.GetAsync($"v1/variants/by-sku/{nonExistentSku}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain(nonExistentSku);
        content.Should().ContainAny("not found", "NotFound");
    }

    [Fact]
    public async Task GetVariantBySku_WithEmptySku_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync("v1/variants/by-sku/");

        // Assert
        // FastEndpoints matches the route with an empty SKU parameter, handler validation returns BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetVariantBySku_WithWhitespaceSku_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync("v1/variants/by-sku/%20%20%20"); // URL encoded spaces

        // Assert
        // The handler validates for empty/whitespace SKU and returns BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetVariantBySku_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var skuWithSpecialChars = "SKU-WITH-@#$%-CHARS";

        // Act
        var response = await Client.GetAsync($"v1/variants/by-sku/{Uri.EscapeDataString(skuWithSpecialChars)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound); // SKU doesn't exist
    }

    #endregion

    #region Anonymous Access Tests

    [Fact]
    public async Task GetVariantBySku_WithoutAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-ANON-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 59.99m, 30);

        // Clear authentication
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(variantId);
        response.Data.Sku.Should().Be(uniqueSku);
    }

    [Fact]
    public async Task GetVariantBySku_WithCustomerRole_ShouldReturnSuccess()
    {
        // Arrange
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-CUSTOMER-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 69.99m, 20);

        // Switch to customer authentication
        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(variantId);
        response.Data.Sku.Should().Be(uniqueSku);
    }

    #endregion

    #region Data Persistence Tests

    [Fact]
    public async Task GetVariantBySku_ShouldReturnDataFromDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-DB-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 149.99m, 75);

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);

        // Verify data matches what was created
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(variantId);
        response.Data.ProductId.Should().Be(productId);
        response.Data.Sku.Should().Be(uniqueSku);
        response.Data.Price.Should().Be(149.99m);
        response.Data.StockQuantity.Should().Be(75);
        response.Data.IsActive.Should().BeTrue();

        // Verify in database directly
        await ExecuteDbContextAsync(async context =>
        {
            var dbVariant = await context.ProductVariants
                .FirstOrDefaultAsync(v => v.Sku == uniqueSku);

            dbVariant.Should().NotBeNull();
            dbVariant!.Sku.Should().Be(uniqueSku);
            dbVariant.Price.Amount.Should().Be(149.99m);
            dbVariant.StockQuantity.Should().Be(75);
            dbVariant.IsActive.Should().BeTrue();
        });
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public async Task GetVariantBySku_WithComplexMetadata_ShouldReturnCompleteData()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-META-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var variantRequest = new
        {
            Sku = uniqueSku,
            Price = 129.99m,
            StockQuantity = 60,
            Metadata = new Dictionary<string, object>
            {
                { "color", "Red" },
                { "size", "XL" },
                { "material", "Polyester" },
                { "weight", 1.5 },
                { "featured", true }
            },
            Attributes = new[]
            {
                new
                {
                    AttributeId = attributeId,
                    Value = "Metadata Test"
                }
            },
            IsActive = true
        };
        var addVariantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>($"v1/products/{productId}/variants", variantRequest);
        AssertApiSuccess(addVariantResponse);

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Metadata.Should().NotBeNull();
        response.Data.Metadata.Should().ContainKey("color");
        response.Data.Metadata.Should().ContainKey("size");
        response.Data.Metadata.Should().ContainKey("material");
        response.Data.Metadata.Should().ContainKey("weight");
        response.Data.Metadata.Should().ContainKey("featured");
    }

    [Fact]
    public async Task GetVariantBySku_WithEmptyMetadata_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-EMPTY-META-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 99.99m, 40);

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Metadata.Should().NotBeNull();
        response.Data.Metadata.Should().BeEmpty();
    }

    #endregion

    #region Attributes Tests

    [Fact]
    public async Task GetVariantBySku_WithAttributes_ShouldReturnAttributeDetails()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple attributes with unique names
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var colorAttributeRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: $"test_color_variant_sku_{uniqueId}",
            displayName: "Color",
            type: "Color",
            isVariant: true);
        var colorAttributeResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", colorAttributeRequest);
        AssertApiSuccess(colorAttributeResponse);
        var colorAttributeId = colorAttributeResponse!.Data.Id;

        var sizeAttributeRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: $"test_size_variant_sku_{uniqueId}",
            displayName: "Size",
            type: "Select",
            isVariant: true);
        var sizeAttributeResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", sizeAttributeRequest);
        AssertApiSuccess(sizeAttributeResponse);
        var sizeAttributeId = sizeAttributeResponse!.Data.Id;

        // Create product
        var productRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var productResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);
        AssertApiSuccess(productResponse);
        var productId = productResponse!.Data.Id;

        // Add variant with multiple attributes
        var uniqueSku = $"SKU-ATTR-{uniqueId.ToUpper()}";
        var variantRequest = new
        {
            Sku = uniqueSku,
            Price = 159.99m,
            StockQuantity = 80,
            Metadata = new Dictionary<string, object>(),
            Attributes = new[]
            {
                new
                {
                    AttributeId = colorAttributeId,
                    Value = "Blue"
                },
                new
                {
                    AttributeId = sizeAttributeId,
                    Value = "Medium"
                }
            },
            IsActive = true
        };
        var addVariantResponse = await PostMultipartApiResponseAsync<AddProductVariantResponseV1>($"v1/products/{productId}/variants", variantRequest);
        AssertApiSuccess(addVariantResponse);

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Attributes.Should().NotBeNull();
        response.Data.Attributes.Should().HaveCount(2);

        var colorAttribute = response.Data.Attributes.FirstOrDefault(a => a.AttributeId == colorAttributeId);
        colorAttribute.Should().NotBeNull();
        colorAttribute!.Value.Should().ContainKey("value");
        colorAttribute.Value["value"]?.ToString().Should().Be("Blue");

        var sizeAttribute = response.Data.Attributes.FirstOrDefault(a => a.AttributeId == sizeAttributeId);
        sizeAttribute.Should().NotBeNull();
        sizeAttribute!.Value.Should().ContainKey("value");
        sizeAttribute.Value["value"]?.ToString().Should().Be("Medium");
    }

    #endregion

    #region Caching Tests

    [Fact]
    public async Task GetVariantBySku_CalledTwice_ShouldReturnConsistentData()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-CACHE-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 119.99m, 55);

        ClearAuthenticationHeader();

        // Act - Call twice
        var firstResponse = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");
        var secondResponse = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(firstResponse);
        AssertApiSuccess(secondResponse);

        firstResponse!.Data.Should().NotBeNull();
        secondResponse!.Data.Should().NotBeNull();

        // Data should be identical
        firstResponse.Data.Id.Should().Be(secondResponse.Data.Id);
        firstResponse.Data.ProductId.Should().Be(secondResponse.Data.ProductId);
        firstResponse.Data.Sku.Should().Be(secondResponse.Data.Sku);
        firstResponse.Data.Price.Should().Be(secondResponse.Data.Price);
        firstResponse.Data.StockQuantity.Should().Be(secondResponse.Data.StockQuantity);
        firstResponse.Data.CreatedAt.Should().Be(secondResponse.Data.CreatedAt);
        firstResponse.Data.UpdatedAt.Should().Be(secondResponse.Data.UpdatedAt);
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public async Task GetVariantBySku_ShouldReturnProperApiResponseFormat()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-FORMAT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 89.99m, 45);

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Message.Should().NotBeNullOrEmpty();
        response.Errors.Should().BeEmpty();
        response.StatusCode.Should().Be(200);
    }

    #endregion

    #region Stock Quantity Tests

    [Fact]
    public async Task GetVariantBySku_WithZeroStock_ShouldReturnCorrectStockQuantity()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-ZERO-STOCK-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 99.99m, 0);

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(0);
    }

    [Fact]
    public async Task GetVariantBySku_WithHighStock_ShouldReturnCorrectStockQuantity()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-HIGH-STOCK-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 99.99m, 9999);

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.StockQuantity.Should().Be(9999);
    }

    #endregion

    #region SKU Format Tests

    [Fact]
    public async Task GetVariantBySku_WithUppercaseSku_ShouldReturnVariant()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-UPPERCASE-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 99.99m, 50);

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be(uniqueSku);
    }

    [Fact]
    public async Task GetVariantBySku_WithHyphenatedSku_ShouldReturnVariant()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-HYPHEN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 99.99m, 50);

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be(uniqueSku);
    }

    [Fact]
    public async Task GetVariantBySku_WithNumericSku_ShouldReturnVariant()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"123456789{Guid.NewGuid().ToString("N")[..4]}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 99.99m, 50);

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be(uniqueSku);
    }

    [Fact]
    public async Task GetVariantBySku_WithAlphanumericSku_ShouldReturnVariant()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"ABC123XYZ{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 99.99m, 50);

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be(uniqueSku);
    }

    #endregion

    #region Multiple Variants Integration Test

    [Fact]
    public async Task GetVariantBySku_MultipleVariantsOfSameProduct_ShouldReturnCorrectIndividualData()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();

        var baseId = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var testVariants = new[]
        {
            ($"SKU-MULTI-A-{baseId}", 99.99m, 50),
            ($"SKU-MULTI-B-{baseId}", 109.99m, 75),
            ($"SKU-MULTI-C-{baseId}", 119.99m, 100)
        };

        var variantIds = new List<Guid>();

        // Create all variants
        foreach (var (sku, price, stock) in testVariants)
        {
            var variantId = await CreateVariantAsync(productId, attributeId, sku, price, stock);
            variantIds.Add(variantId);
        }

        ClearAuthenticationHeader();

        // Act & Assert - Retrieve and verify each variant by SKU
        for (int i = 0; i < testVariants.Length; i++)
        {
            var (expectedSku, expectedPrice, expectedStock) = testVariants[i];
            var variantId = variantIds[i];

            var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{expectedSku}");
            AssertApiSuccess(response);

            response!.Data.Should().NotBeNull();
            response.Data.Id.Should().Be(variantId);
            response.Data.ProductId.Should().Be(productId);
            response.Data.Sku.Should().Be(expectedSku);
            response.Data.Price.Should().Be(expectedPrice);
            response.Data.StockQuantity.Should().Be(expectedStock);
            response.Data.IsActive.Should().BeTrue();
        }
    }

    #endregion

    #region Inactive Variant Tests

    [Fact]
    public async Task GetVariantBySku_InactiveVariant_ShouldStillReturnVariant()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-INACTIVE-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 99.99m, 25);

        // Deactivate the variant via UpdateVariantStatus endpoint
        var updateStatusRequest = new { IsActive = false };
        var updateResponse = await PutAsync($"v1/variants/{variantId}/status", updateStatusRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{uniqueSku}");

        // Assert - GetVariantBySku should return inactive variants (filtering happens at list level)
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.IsActive.Should().BeFalse();
        response.Data.Sku.Should().Be(uniqueSku);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public async Task GetVariantBySku_ShouldBeCaseSensitive()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU-CASE-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 99.99m, 50);

        ClearAuthenticationHeader();

        // Act - Try with lowercase SKU (different case)
        var lowercaseSku = uniqueSku.ToLower();
        var response = await Client.GetAsync($"v1/variants/by-sku/{lowercaseSku}");

        // Assert - Should not find the variant (SKU is case-sensitive in most systems)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region URL Encoding Tests

    [Fact]
    public async Task GetVariantBySku_WithUrlEncodedSku_ShouldReturnVariant()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        var uniqueSku = $"SKU+WITH+PLUS-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 99.99m, 50);

        ClearAuthenticationHeader();

        // Act - SKU with + should be URL encoded
        var encodedSku = Uri.EscapeDataString(uniqueSku);
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{encodedSku}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be(uniqueSku);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetVariantBySku_WithLongSku_ShouldReturnVariant()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var (productId, attributeId) = await CreateProductAndAttributeForVariantAsync();
        // Create a long SKU (close to max length)
        var uniqueSku = $"SKU-LONG-{new string('X', 80)}-{Guid.NewGuid().ToString("N")[..8]}";
        var variantId = await CreateVariantAsync(productId, attributeId, uniqueSku, 99.99m, 50);

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<ProductVariantDto>($"v1/variants/by-sku/{Uri.EscapeDataString(uniqueSku)}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be(uniqueSku);
    }

    #endregion
}
