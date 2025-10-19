using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;
using Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;
using Shopilent.API.Endpoints.Catalog.Categories.CreateCategory.V1;
using Shopilent.API.Endpoints.Catalog.Products.CreateProduct.V1;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Products.CreateProduct.V1;

public class CreateProductEndpointV1Tests : ApiIntegrationTestBase
{
    public CreateProductEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Creation.CreateValidRequest(
            name: "Test Product",
            slug: "test-product",
            description: "Test product description",
            basePrice: 99.99m);

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().NotBeEmpty();
        response.Data.Name.Should().Be("Test Product");
        response.Data.Slug.Should().Be("test-product");
        response.Data.Description.Should().Be("Test product description");
        response.Data.BasePrice.Should().Be(99.99m);
        response.Data.Currency.Should().Be("USD");
        response.Data.IsActive.Should().BeTrue();
        response.Data.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldCreateProductInDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Creation.CreateValidRequest(
            name: "Database Test Product",
            slug: "database-test-product",
            basePrice: 49.99m,
            sku: "TEST-SKU-001");

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);

        // Verify product exists in database
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == response!.Data.Id);

            product.Should().NotBeNull();
            product!.Name.Should().Be("Database Test Product");
            product.Slug.Value.Should().Be("database-test-product");
            product.BasePrice.Amount.Should().Be(49.99m);
            product.Sku.Should().Be("TEST-SKU-001");
            product.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public async Task CreateProduct_WithMinimalData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.BoundaryTests.CreateRequestWithMinimumValidData();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("A");
        response.Data.Slug.Should().Be("a");
        response.Data.BasePrice.Should().Be(0m);
    }

    [Fact]
    public async Task CreateProduct_WithCategories_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a test category first
        var categoryRequest = CategoryTestDataV1.Creation.CreateValidRequest(
            name: "Test Category For Product",
            slug: "test-category-for-product");
        var categoryResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", categoryRequest);
        AssertApiSuccess(categoryResponse);
        var categoryId = categoryResponse!.Data.Id;

        // Create product with category
        var productRequest = ProductTestDataV1.Creation.CreateProductWithCategories(new List<Guid> { categoryId });

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.CategoryIds.Should().ContainSingle();
        response.Data.CategoryIds.Should().Contain(categoryId);
    }

    [Fact]
    public async Task CreateProduct_WithAttributes_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a test attribute first
        var attributeRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: "test_attr_for_product",
            displayName: "Test Attribute For Product",
            type: "Text");
        var attributeResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);
        AssertApiSuccess(attributeResponse);
        var attributeId = attributeResponse!.Data.Id;

        // Create product with attribute
        var productRequest = ProductTestDataV1.Creation.CreateProductWithAttributes(new List<Guid> { attributeId });

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", productRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().NotBeEmpty();

        // Verify product with attributes was created in database
        await ExecuteDbContextAsync(async context =>
        {
            var product = await context.Products
                .Include(p => p.Attributes)
                .FirstOrDefaultAsync(p => p.Id == response.Data.Id);

            product.Should().NotBeNull();
            product!.Attributes.Should().NotBeEmpty();
            product.Attributes.Should().ContainSingle();
            product.Attributes.First().AttributeId.Should().Be(attributeId);
        });
    }

    [Fact]
    public async Task CreateProduct_WithManagerRole_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Creation.CreateValidRequest(
            name: "Manager Product",
            slug: "manager-product");

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("Manager Product");
    }

    [Fact]
    public async Task CreateProduct_WithInactiveStatus_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.EdgeCases.CreateInactiveProductRequest();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsActive.Should().BeFalse();
    }

    #endregion

    #region Validation Tests - Name

    [Fact]
    public async Task CreateProduct_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithEmptyName();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Product name is required");
    }

    [Fact]
    public async Task CreateProduct_WithNullName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithNullName();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Product name is required");
    }

    [Fact]
    public async Task CreateProduct_WithWhitespaceName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithWhitespaceName();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Product name is required");
    }

    [Fact]
    public async Task CreateProduct_WithExcessiveNameLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithLongName();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

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
    public async Task CreateProduct_WithEmptySlug_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithEmptySlug();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Product slug is required");
    }

    [Fact]
    public async Task CreateProduct_WithNullSlug_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithNullSlug();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Product slug is required");
    }

    [Fact]
    public async Task CreateProduct_WithInvalidSlug_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithInvalidSlug();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("lowercase letters, numbers, and hyphens");
    }

    [Fact]
    public async Task CreateProduct_WithUppercaseSlug_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithUppercaseSlug();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("lowercase letters, numbers, and hyphens");
    }

    [Fact]
    public async Task CreateProduct_WithExcessiveSlugLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithLongSlug();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

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
    public async Task CreateProduct_WithNegativePrice_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithNegativePrice();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Base price cannot be negative");
    }

    [Fact]
    public async Task CreateProduct_WithZeroPrice_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.BoundaryTests.CreateRequestWithZeroPrice();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.BasePrice.Should().Be(0m);
    }

    #endregion

    #region Validation Tests - Currency

    [Fact]
    public async Task CreateProduct_WithEmptyCurrency_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithEmptyCurrency();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Currency is required");
    }

    [Fact]
    public async Task CreateProduct_WithInvalidCurrencyLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithInvalidCurrencyLength();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Currency code must be 3 characters");
    }

    #endregion

    #region Validation Tests - SKU and Description

    [Fact]
    public async Task CreateProduct_WithExcessiveSkuLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithLongSku();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("100 characters");
        content.Should().Contain("SKU");
    }

    [Fact]
    public async Task CreateProduct_WithExcessiveDescriptionLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithLongDescription();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("2000 characters");
        content.Should().Contain("description");
    }

    #endregion

    #region Validation Tests - Attributes

    [Fact]
    public async Task CreateProduct_WithEmptyAttributeId_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Validation.CreateRequestWithEmptyAttributeId();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Attribute ID is required");
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task CreateProduct_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ProductTestDataV1.Creation.CreateValidRequest();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Creation.CreateValidRequest();

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Conflict Tests

    [Fact]
    public async Task CreateProduct_WithDuplicateSlug_ShouldReturnConflict()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var slug = "duplicate-product-slug";
        var firstRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: "First Product",
            slug: slug);
        var secondRequest = ProductTestDataV1.Creation.CreateValidRequest(
            name: "Second Product",
            slug: slug);

        // Act - Create first product
        var firstResponse = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", firstRequest);
        AssertApiSuccess(firstResponse);

        // Act - Try to create second product with same slug
        var secondResponse = await PostMultipartAsync("v1/products", secondRequest);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var content = await secondResponse.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("already exists", "duplicate", "slug");
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task CreateProduct_WithMaximumNameLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.BoundaryTests.CreateRequestWithMaximumNameLength();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Length.Should().Be(255);
    }

    [Fact]
    public async Task CreateProduct_WithMaximumSlugLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.BoundaryTests.CreateRequestWithMaximumSlugLength();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Slug.Length.Should().Be(255);
    }

    [Fact]
    public async Task CreateProduct_WithMaximumDescriptionLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.BoundaryTests.CreateRequestWithMaximumDescriptionLength();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Description.Length.Should().Be(2000);
    }

    [Fact]
    public async Task CreateProduct_WithMaximumSkuLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.BoundaryTests.CreateRequestWithMaximumSkuLength();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Length.Should().Be(100);
    }

    [Fact]
    public async Task CreateProduct_WithMinimumValidName_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.BoundaryTests.CreateRequestWithMinimumValidName();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("A");
    }

    [Fact]
    public async Task CreateProduct_WithMinimumValidSlug_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.BoundaryTests.CreateRequestWithMinimumValidSlug();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Slug.Should().Be("a");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task CreateProduct_WithUnicodeCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.EdgeCases.CreateRequestWithUnicodeCharacters();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("Café Münchën Product™");
        response.Data.Description.Should().Contain("émojis 🛍️");
    }

    [Fact]
    public async Task CreateProduct_WithSpecialCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.EdgeCases.CreateRequestWithSpecialCharacters();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("Product-With_Special.Chars@123");
    }

    [Fact]
    public async Task CreateProduct_WithEmptyDescription_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.EdgeCases.CreateRequestWithEmptyDescription();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Description.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task CreateProduct_WithNullDescription_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.EdgeCases.CreateRequestWithNullDescription();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Description.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task CreateProduct_WithNullSku_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.EdgeCases.CreateRequestWithNullSku();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Sku.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task CreateProduct_WithComplexMetadata_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.EdgeCases.CreateRequestWithComplexMetadata();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Metadata.Should().NotBeEmpty();
        response.Data.Metadata.Should().ContainKey("brand");
    }

    [Fact]
    public async Task CreateProduct_WithEmptyCollections_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.EdgeCases.CreateRequestWithEmptyCollections();

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.CategoryIds.Should().BeEmpty();
        // Note: Attributes is not mapped in endpoint response, so it will be null
        response.Data.Attributes.Should().BeNullOrEmpty();
    }

    #endregion

    #region Currency Tests

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    public async Task CreateProduct_WithVariousCurrencies_ShouldReturnSuccess(string currency)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Creation.CreateValidRequest(currency: currency);

        // Act
        var response = await PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Currency.Should().Be(currency);
    }

    #endregion

    #region Slug Format Tests

    [Theory]
    [InlineData("UPPERCASE")]
    [InlineData("spaces in slug")]
    [InlineData("special@characters!")]
    [InlineData("under_scores")]
    public async Task CreateProduct_WithInvalidSlugFormats_ShouldReturnValidationError(string invalidSlug)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = ProductTestDataV1.Creation.CreateValidRequest(slug: invalidSlug);

        // Act
        var response = await PostMultipartAsync("v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("lowercase letters, numbers, and hyphens");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task CreateProduct_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var tasks = Enumerable.Range(0, 10)
            .Select(i => ProductTestDataV1.Creation.CreateValidRequest(
                name: $"Concurrent Product {i}",
                slug: $"concurrent-product-{i}-{Guid.NewGuid():N}"))
            .Select(request => PostMultipartApiResponseAsync<CreateProductResponseV1>("v1/products", request))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Select(r => r!.Data.Id).Should().OnlyHaveUniqueItems();
        responses.Select(r => r!.Data.Slug).Should().OnlyHaveUniqueItems();
    }

    #endregion
}
