using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;
using Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;
using Shopilent.API.Endpoints.Catalog.Categories.CreateCategory.V1;
using Shopilent.API.Endpoints.Catalog.Products.CreateProduct.V1;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Products.UpdateProduct.V1;

public class UpdateProductEndpointV1Tests : ApiIntegrationTestBase
{
    public UpdateProductEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task UpdateProduct_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product first
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: "Original Product",
            slug: "original-product",
            basePrice: 100m);
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update request
        var updateRequest = new
        {
            Name = "Updated Product Name",
            Slug = "updated-product-name",
            Description = "Updated description for the product",
            BasePrice = 150m,
            Sku = "UPDATED-SKU-001",
            IsActive = true
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(productId);
        response.Data.Name.Should().Be("Updated Product Name");
        response.Data.Slug.Should().Be("updated-product-name");
        response.Data.Description.Should().Be("Updated description for the product");
        response.Data.BasePrice.Should().Be(150m);
        response.Data.Sku.Should().Be("UPDATED-SKU-001");
        response.Data.IsActive.Should().BeTrue();
        response.Data.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ShouldUpdateInDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product first
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: "Database Test Product",
            slug: "database-test-product",
            basePrice: 99.99m);
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update request
        var updateRequest = new
        {
            Name = "Updated Database Product",
            Slug = "updated-database-product",
            Description = "Updated database description",
            BasePrice = 199.99m,
            Sku = "DB-UPDATE-SKU",
            IsActive = true
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            product.Should().NotBeNull();
            product!.Name.Should().Be("Updated Database Product");
            product.Slug.Value.Should().Be("updated-database-product");
            product.Description.Should().Be("Updated database description");
            product.BasePrice.Amount.Should().Be(199.99m);
            product.Sku.Should().Be("DB-UPDATE-SKU");
            product.IsActive.Should().BeTrue();
            product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        });
    }

    [Fact]
    public async Task UpdateProduct_WithMinimalData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a product first
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with minimal data
        var updateRequest = new
        {
            Name = "M",
            Slug = "m",
            Description = (string?)null,
            BasePrice = 0m,
            Sku = (string?)null,
            IsActive = false
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("M");
        response.Data.Slug.Should().Be("m");
        response.Data.BasePrice.Should().Be(0m);
        response.Data.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateProduct_WithCategories_ShouldUpdateSuccessfully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create categories
        var category1Request = CategoryTestDataV1.Creation.CreateValidRequest(
            name: "Category 1",
            slug: "category-1-update");
        var category1Response = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", category1Request);
        AssertApiSuccess(category1Response);
        var category1Id = category1Response!.Data.Id;

        var category2Request = CategoryTestDataV1.Creation.CreateValidRequest(
            name: "Category 2",
            slug: "category-2-update");
        var category2Response = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", category2Request);
        AssertApiSuccess(category2Response);
        var category2Id = category2Response!.Data.Id;

        // Create product with category1
        var createRequest = ProductTestDataV1.Creation.CreateProductWithCategories(new List<Guid> { category1Id });
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update to use category2 instead
        var updateRequest = new
        {
            Name = "Updated Product with Categories",
            Slug = "updated-product-categories",
            Description = "Product with updated categories",
            BasePrice = 50m,
            CategoryIds = new List<Guid> { category2Id }
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.CategoryIds.Should().ContainSingle();
        response.Data.CategoryIds.Should().Contain(category2Id);
        response.Data.CategoryIds.Should().NotContain(category1Id);
    }

    [Fact]
    public async Task UpdateProduct_WithAttributes_ShouldUpdateSuccessfully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create attributes
        var attribute1Request = AttributeTestDataV1.Creation.CreateValidRequest(
            name: "attr_1_update",
            displayName: "Attribute 1",
            type: "Text");
        var attribute1Response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attribute1Request);
        AssertApiSuccess(attribute1Response);
        var attribute1Id = attribute1Response!.Data.Id;

        var attribute2Request = AttributeTestDataV1.Creation.CreateValidRequest(
            name: "attr_2_update",
            displayName: "Attribute 2",
            type: "Number");
        var attribute2Response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attribute2Request);
        AssertApiSuccess(attribute2Response);
        var attribute2Id = attribute2Response!.Data.Id;

        // Create product with attribute1
        var createRequest = ProductTestDataV1.Creation.CreateProductWithAttributes(new List<Guid> { attribute1Id });
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update to use attribute2 instead
        var updateRequest = new
        {
            Name = "Updated Product with Attributes",
            Slug = "updated-product-attributes",
            Description = "Product with updated attributes",
            BasePrice = 75m,
            Attributes = new[]
            {
                new
                {
                    AttributeId = attribute2Id,
                    Value = "42"
                }
            }
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();

        // Verify in database
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products
                .Include(p => p.Attributes)
                .FirstOrDefaultAsync(p => p.Id == productId);

            product.Should().NotBeNull();
            product!.Attributes.Should().ContainSingle();
            product.Attributes.First().AttributeId.Should().Be(attribute2Id);
        });
    }

    [Fact]
    public async Task UpdateProduct_WithManagerRole_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Re-authenticate as manager
        var managerToken = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(managerToken);

        // Update request
        var updateRequest = new
        {
            Name = "Manager Updated Product",
            Slug = "manager-updated-product",
            Description = "Updated by manager",
            BasePrice = 125m
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("Manager Updated Product");
    }

    [Fact]
    public async Task UpdateProduct_WithUnicodeCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with unicode
        var updateRequest = new
        {
            Name = "Café Münchën Product™",
            Slug = "cafe-munchen-product",
            Description = "Ürünümüz için açıklama with émojis 🛍️",
            BasePrice = 99.99m
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("Café Münchën Product™");
        response.Data.Description.Should().Contain("émojis 🛍️");
    }

    [Fact]
    public async Task UpdateProduct_ToInactiveStatus_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create active product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update to inactive
        var updateRequest = new
        {
            Name = "Inactive Product",
            Slug = "inactive-product-update",
            Description = "This product is now inactive",
            BasePrice = 50m,
            IsActive = false
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsActive.Should().BeFalse();
    }

    #endregion

    #region Validation Tests - Name

    [Fact]
    public async Task UpdateProduct_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with empty name
        var updateRequest = new
        {
            Name = "",
            Slug = "valid-slug",
            Description = "Valid description",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Product name is required");
    }

    [Fact]
    public async Task UpdateProduct_WithNullName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with null name
        var updateRequest = new
        {
            Name = (string?)null,
            Slug = "valid-slug",
            Description = "Valid description",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Product name is required");
    }

    [Fact]
    public async Task UpdateProduct_WithExcessiveNameLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with long name
        var updateRequest = new
        {
            Name = new string('A', 256), // Exceeds 255
            Slug = "valid-slug",
            Description = "Valid description",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("255 characters");
        content.Should().Contain("name");
    }

    #endregion

    #region Validation Tests - Slug

    [Fact]
    public async Task UpdateProduct_WithEmptySlug_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with empty slug
        var updateRequest = new
        {
            Name = "Valid Name",
            Slug = "",
            Description = "Valid description",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Product slug is required");
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidSlug_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with invalid slug
        var updateRequest = new
        {
            Name = "Valid Name",
            Slug = "Invalid Slug With Spaces!",
            Description = "Valid description",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("lowercase letters, numbers, and hyphens");
    }

    [Fact]
    public async Task UpdateProduct_WithUppercaseSlug_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with uppercase slug
        var updateRequest = new
        {
            Name = "Valid Name",
            Slug = "UPPERCASE-SLUG",
            Description = "Valid description",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("lowercase letters, numbers, and hyphens");
    }

    [Fact]
    public async Task UpdateProduct_WithExcessiveSlugLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with long slug
        var updateRequest = new
        {
            Name = "Valid Name",
            Slug = new string('a', 256), // Exceeds 255
            Description = "Valid description",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("255 characters");
        content.Should().Contain("slug");
    }

    #endregion

    #region Validation Tests - Price

    [Fact]
    public async Task UpdateProduct_WithNegativePrice_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with negative price
        var updateRequest = new
        {
            Name = "Valid Name",
            Slug = "valid-slug",
            Description = "Valid description",
            BasePrice = -10.50m
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("greater than or equal to zero");
    }

    [Fact]
    public async Task UpdateProduct_WithZeroPrice_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with zero price
        var updateRequest = new
        {
            Name = "Free Product",
            Slug = "free-product-update",
            Description = "This product is free",
            BasePrice = 0m
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.BasePrice.Should().Be(0m);
    }

    #endregion

    #region Validation Tests - SKU

    [Fact]
    public async Task UpdateProduct_WithExcessiveSkuLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with long SKU
        var updateRequest = new
        {
            Name = "Valid Name",
            Slug = "valid-slug",
            Description = "Valid description",
            BasePrice = 50m,
            Sku = new string('S', 101) // Exceeds 100
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("100 characters");
        content.Should().Contain("SKU");
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task UpdateProduct_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var productId = Guid.NewGuid();
        var updateRequest = new
        {
            Name = "Test Name",
            Slug = "test-slug",
            Description = "Test description",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProduct_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var productId = Guid.NewGuid();
        var updateRequest = new
        {
            Name = "Test Name",
            Slug = "test-slug",
            Description = "Test description",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task UpdateProduct_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var nonExistentId = Guid.NewGuid();
        var updateRequest = new
        {
            Name = "Test Name",
            Slug = "test-slug",
            Description = "Test description",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var updateRequest = new
        {
            Name = "Test Name",
            Slug = "test-slug",
            Description = "Test description",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartAsync("v1/products/invalid-guid", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProduct_WithDuplicateSlug_ShouldReturnConflict()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create two products
        var product1Request = ProductTestDataV1.Creation.CreateValidRequest(
            name: "Product 1",
            slug: "product-1-unique");
        var product1Response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", product1Request);
        AssertApiSuccess(product1Response);

        var product2Request = ProductTestDataV1.Creation.CreateValidRequest(
            name: "Product 2",
            slug: "product-2-unique");
        var product2Response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", product2Request);
        AssertApiSuccess(product2Response);
        var product2Id = product2Response!.Data.Id;

        // Try to update product2 with product1's slug
        var updateRequest = new
        {
            Name = "Updated Product 2",
            Slug = "product-1-unique", // This slug already exists
            Description = "Updated description",
            BasePrice = 75m
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{product2Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("already exists", "duplicate", "slug");
    }

    [Fact]
    public async Task UpdateProduct_WithDuplicateSku_ShouldReturnConflict()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create two products
        var product1Request = ProductTestDataV1.Creation.CreateValidRequest(
            name: "Product 1",
            slug: "product-1-sku-test",
            sku: "UNIQUE-SKU-001");
        var product1Response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", product1Request);
        AssertApiSuccess(product1Response);

        var product2Request = ProductTestDataV1.Creation.CreateValidRequest(
            name: "Product 2",
            slug: "product-2-sku-test",
            sku: "UNIQUE-SKU-002");
        var product2Response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", product2Request);
        AssertApiSuccess(product2Response);
        var product2Id = product2Response!.Data.Id;

        // Try to update product2 with product1's SKU
        var updateRequest = new
        {
            Name = "Updated Product 2",
            Slug = "updated-product-2-sku",
            Description = "Updated description",
            BasePrice = 75m,
            Sku = "UNIQUE-SKU-001" // This SKU already exists
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{product2Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("already exists", "duplicate", "SKU");
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task UpdateProduct_WithMaximumNameLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with maximum name length
        var updateRequest = new
        {
            Name = new string('A', 255), // Exactly 255
            Slug = "max-name-length",
            Description = "Maximum name length test",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Length.Should().Be(255);
    }

    [Fact]
    public async Task UpdateProduct_WithMaximumSlugLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with maximum slug length
        var updateRequest = new
        {
            Name = "Maximum Slug Length",
            Slug = new string('a', 255), // Exactly 255
            Description = "Maximum slug length test",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Slug.Length.Should().Be(255);
    }

    [Fact]
    public async Task UpdateProduct_WithMaximumSkuLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with maximum SKU length
        var updateRequest = new
        {
            Name = "Maximum SKU Length",
            Slug = "max-sku-length",
            Description = "Maximum SKU length test",
            BasePrice = 50m,
            Sku = new string('S', 100) // Exactly 100
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku!.Length.Should().Be(100);
    }

    [Fact]
    public async Task UpdateProduct_WithMinimumValidName_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with minimum name
        var updateRequest = new
        {
            Name = "A", // Single character
            Slug = "a",
            Description = "Minimum name test",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("A");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task UpdateProduct_WithSpecialCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with special characters
        var updateRequest = new
        {
            Name = "Product-With_Special.Chars@123",
            Slug = "product-with-special-chars-123",
            Description = "Description with special characters: !@#$%^&*()",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("Product-With_Special.Chars@123");
    }

    [Fact]
    public async Task UpdateProduct_WithEmptyDescription_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with empty description
        var updateRequest = new
        {
            Name = "Product Without Description",
            Slug = "product-no-desc",
            Description = "",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Description.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateProduct_WithNullSku_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product with SKU
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest(sku: "ORIGINAL-SKU");
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update to remove SKU
        var updateRequest = new
        {
            Name = "Product Without SKU",
            Slug = "product-no-sku-update",
            Description = "Product without SKU",
            BasePrice = 50m,
            Sku = (string?)null
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateProduct_KeepingSameSlug_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: "Original Product",
            slug: "same-slug-test");
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update but keep the same slug
        var updateRequest = new
        {
            Name = "Updated Product Name",
            Slug = "same-slug-test", // Same slug as before
            Description = "Updated description",
            BasePrice = 75m
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Slug.Should().Be("same-slug-test");
    }

    [Fact]
    public async Task UpdateProduct_KeepingSameSku_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: "Original Product",
            slug: "same-sku-test",
            sku: "SAME-SKU-001");
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update but keep the same SKU
        var updateRequest = new
        {
            Name = "Updated Product Name",
            Slug = "updated-slug-sku-test",
            Description = "Updated description",
            BasePrice = 75m,
            Sku = "SAME-SKU-001" // Same SKU as before
        };

        // Act
        var response = await PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().Be("SAME-SKU-001");
    }

    #endregion

    #region Slug Format Tests

    [Theory]
    [InlineData("UPPERCASE")]
    [InlineData("spaces in slug")]
    [InlineData("special@characters!")]
    [InlineData("under_scores")]
    public async Task UpdateProduct_WithInvalidSlugFormats_ShouldReturnValidationError(string invalidSlug)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create product
        var createRequest = ProductTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
        AssertApiSuccess(createResponse);
        var productId = createResponse!.Data.Id;

        // Update with invalid slug
        var updateRequest = new
        {
            Name = "Valid Name",
            Slug = invalidSlug,
            Description = "Valid description",
            BasePrice = 50m
        };

        // Act
        var response = await PutMultipartAsync($"v1/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("lowercase letters, numbers, and hyphens");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task UpdateProduct_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple products
        var productIds = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var createRequest = ProductTestDataV1.Creation.CreateValidRequest(
                name: $"Concurrent Product {i}",
                slug: $"concurrent-product-{i}-{Guid.NewGuid():N}");
            var createResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", createRequest);
            AssertApiSuccess(createResponse);
            productIds.Add(createResponse!.Data.Id);
        }

        // Update all concurrently
        var tasks = productIds.Select((id, index) => new
        {
            Name = $"Updated Concurrent Product {index}",
            Slug = $"updated-concurrent-product-{index}-{Guid.NewGuid():N}",
            Description = $"Updated concurrent product {index}",
            BasePrice = 99.99m + index
        })
        .Select((request, index) => PutMultipartApiResponseAsync<UpdateProductResponseV1>($"v1/products/{productIds[index]}", request))
        .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Select(r => r!.Data.Id).Should().OnlyHaveUniqueItems();
        responses.Should().AllSatisfy(response =>
            response!.Data.Name.Should().StartWith("Updated Concurrent Product"));
    }

    #endregion

    // Response DTO for this specific endpoint version
    public class UpdateProductResponseV1
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public bool IsActive { get; set; }
        public List<Guid> CategoryIds { get; set; } = new();
        public List<ProductAttributeResponseDto> Attributes { get; set; } = new();
        public List<ProductImageResponseDto> Images { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
    }

    public class ProductAttributeResponseDto
    {
        public Guid AttributeId { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public object Values { get; set; } = new();
    }

    public class ProductImageResponseDto
    {
        public string Url { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public int DisplayOrder { get; set; }
    }
}
