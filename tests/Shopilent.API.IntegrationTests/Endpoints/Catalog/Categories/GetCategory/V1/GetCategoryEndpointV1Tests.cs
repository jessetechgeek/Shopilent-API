using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.GetCategory.V1;

public class GetCategoryEndpointV1Tests : ApiIntegrationTestBase
{
    public GetCategoryEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task GetCategory_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a test category first
        var createRequest = CategoryTestDataV1.Creation.CreateValidRequest(
            name: "test_get_category",
            slug: "test-get-category",
            description: "Test Get Category");
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Clear auth header to test anonymous access
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<CategoryDto>($"v1/categories/{categoryId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(categoryId);
        response.Data.Name.Should().Be("test_get_category");
        response.Data.Description.Should().Be("Test Get Category");
        response.Data.IsActive.Should().BeTrue();
        response.Data.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        response.Data.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetCategory_WithValidId_ShouldReturnCorrectHierarchyData()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a root category first
        var rootRequest = CategoryTestDataV1.Hierarchical.CreateRootCategory();
        var rootResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", rootRequest);
        AssertApiSuccess(rootResponse);
        var rootCategoryId = rootResponse!.Data.Id;

        // Create a child category
        var childRequest = CategoryTestDataV1.Hierarchical.CreateChildCategory(rootCategoryId);
        var childResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", childRequest);
        AssertApiSuccess(childResponse);
        var childCategoryId = childResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<CategoryDto>($"v1/categories/{childCategoryId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(childCategoryId);
        response.Data.Name.Should().Be("child_category_test");
        response.Data.ParentId.Should().Be(rootCategoryId);
        response.Data.Level.Should().BeGreaterThan(0);
        response.Data.Path.Should().NotBeNullOrEmpty();
        response.Data.Slug.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCategory_CreatedCategory_ShouldBeActiveByDefault()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = CategoryTestDataV1.Creation.CreateValidRequest(
            name: "test_active_category",
            slug: "test-active-category");
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<CategoryDto>($"v1/categories/{categoryId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.IsActive.Should().BeTrue(); // Categories are active by default
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetCategory_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"v1/categories/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain(nonExistentId.ToString());
        content.Should().ContainAny("not found", "NotFound");
    }

    [Fact]
    public async Task GetCategory_WithInvalidGuidFormat_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync("v1/categories/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCategory_WithEmptyGuid_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.GetAsync($"v1/categories/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Anonymous Access Tests

    [Fact]
    public async Task GetCategory_WithoutAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a test category
        var createRequest = CategoryTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Clear authentication
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<CategoryDto>($"v1/categories/{categoryId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(categoryId);
    }

    [Fact]
    public async Task GetCategory_WithCustomerRole_ShouldReturnSuccess()
    {
        // Arrange

        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        // Create a test category as admin
        var createRequest = CategoryTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Switch to customer authentication
        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);

        // Act
        var response = await GetApiResponseAsync<CategoryDto>($"v1/categories/{categoryId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(categoryId);
    }

    #endregion

    #region Unicode and Special Characters Tests

    [Fact]
    public async Task GetCategory_WithUnicodeCharacters_ShouldReturnCorrectData()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = CategoryTestDataV1.EdgeCases.CreateRequestWithUnicodeCharactersForGetCategory();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<CategoryDto>($"v1/categories/{categoryId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Name.Should().Be("Café & Münchën Category™");
        response.Data.Description.Should().Be("Ürünler için kategori with émojis 🛍️");
    }

    [Fact]
    public async Task GetCategory_WithSpecialCharacters_ShouldReturnCorrectData()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = CategoryTestDataV1.EdgeCases.CreateRequestWithSpecialCharacters();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<CategoryDto>($"v1/categories/{categoryId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Name.Should().Be("Category-With_Special.Chars@123");
        response.Data.Description.Should().Be("Description with special characters: !@#$%^&*()");
    }

    #endregion

    #region Data Persistence Tests

    [Fact]
    public async Task GetCategory_ShouldReturnDataFromDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = CategoryTestDataV1.Creation.CreateValidRequest(
            name: "db_persistence_test",
            slug: "db-persistence-test",
            description: "Database Persistence Test Category");
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<CategoryDto>($"v1/categories/{categoryId}");

        // Assert
        AssertApiSuccess(response);

        // Verify data matches what was created
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(categoryId);
        response.Data.Name.Should().Be("db_persistence_test");
        response.Data.Description.Should().Be("Database Persistence Test Category");
        response.Data.IsActive.Should().BeTrue();

        // Verify in database directly
        await ExecuteDbContextAsync(async context =>
        {
            var dbCategory = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            dbCategory.Should().NotBeNull();
            dbCategory!.Name.Should().Be("db_persistence_test");
            dbCategory.Description.Should().Be("Database Persistence Test Category");
            dbCategory.IsActive.Should().BeTrue();
        });
    }

    #endregion

    #region Hierarchy Tests

    [Fact]
    public async Task GetCategory_RootCategory_ShouldHaveCorrectHierarchyData()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = CategoryTestDataV1.Hierarchical.CreateRootCategory();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<CategoryDto>($"v1/categories/{categoryId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.ParentId.Should().BeNull();
        response.Data.Level.Should().Be(0);
        response.Data.Path.Should().NotBeNullOrEmpty();
        response.Data.Slug.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCategory_ChildCategory_ShouldHaveCorrectHierarchyData()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create root category
        var rootRequest = CategoryTestDataV1.Hierarchical.CreateRootCategory();
        var rootResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", rootRequest);
        AssertApiSuccess(rootResponse);
        var rootCategoryId = rootResponse!.Data.Id;

        // Create child category
        var childRequest = CategoryTestDataV1.Hierarchical.CreateChildCategory(rootCategoryId);
        var childResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", childRequest);
        AssertApiSuccess(childResponse);
        var childCategoryId = childResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<CategoryDto>($"v1/categories/{childCategoryId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.ParentId.Should().Be(rootCategoryId);
        response.Data.Level.Should().Be(1);
        response.Data.Path.Should().NotBeNullOrEmpty();
        response.Data.Path.Should().Contain("root-category-test");
        response.Data.Path.Should().Contain("child-category-test");
    }

    #endregion

    #region Caching Tests

    [Fact]
    public async Task GetCategory_CalledTwice_ShouldReturnConsistentData()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = CategoryTestDataV1.Creation.CreateValidRequest(
            name: "cache_consistency_test",
            slug: "cache-consistency-test");
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act - Call twice
        var firstResponse = await GetApiResponseAsync<CategoryDto>($"v1/categories/{categoryId}");
        var secondResponse = await GetApiResponseAsync<CategoryDto>($"v1/categories/{categoryId}");

        // Assert
        AssertApiSuccess(firstResponse);
        AssertApiSuccess(secondResponse);

        firstResponse!.Data.Should().NotBeNull();
        secondResponse!.Data.Should().NotBeNull();

        // Data should be identical
        firstResponse.Data.Id.Should().Be(secondResponse.Data.Id);
        firstResponse.Data.Name.Should().Be(secondResponse.Data.Name);
        firstResponse.Data.Description.Should().Be(secondResponse.Data.Description);
        firstResponse.Data.ParentId.Should().Be(secondResponse.Data.ParentId);
        firstResponse.Data.Level.Should().Be(secondResponse.Data.Level);
        firstResponse.Data.CreatedAt.Should().Be(secondResponse.Data.CreatedAt);
        firstResponse.Data.UpdatedAt.Should().Be(secondResponse.Data.UpdatedAt);
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public async Task GetCategory_ShouldReturnProperApiResponseFormat()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = CategoryTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<CategoryDto>($"v1/categories/{categoryId}");

        // Assert
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Message.Should().NotBeNullOrEmpty();
        response.Errors.Should().BeEmpty();
        response.StatusCode.Should().Be(200);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task GetCategory_WithLongName_ShouldReturnCorrectData()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = CategoryTestDataV1.BoundaryTests.CreateRequestWithMaximumNameLength();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<CategoryDto>($"v1/categories/{categoryId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Name.Should().HaveLength(100);
        response.Data.Name.Should().Be(new string('A', 100));
    }

    [Fact]
    public async Task GetCategory_WithLongDescription_ShouldReturnCorrectData()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = CategoryTestDataV1.BoundaryTests.CreateRequestWithMaximumDescriptionLength();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<CategoryDto>($"v1/categories/{categoryId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Description.Should().HaveLength(500);
        response.Data.Description.Should().Be(new string('B', 500));
    }

    #endregion

    #region Multiple Categories Integration Test

    [Fact]
    public async Task GetCategory_MultipleCategories_ShouldReturnCorrectIndividualData()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testCases = new[]
        {
            ("Electronics", CategoryTestDataV1.CommerceCategories.CreateElectronicsCategoryRequest()),
            ("Clothing", CategoryTestDataV1.CommerceCategories.CreateClothingCategoryRequest()),
            ("Books", CategoryTestDataV1.CommerceCategories.CreateBooksCategoryRequest())
        };

        var categoryIds = new List<Guid>();

        // Create all categories
        foreach (var (name, request) in testCases)
        {
            var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);
            AssertApiSuccess(createResponse);
            categoryIds.Add(createResponse!.Data.Id);
        }

        ClearAuthenticationHeader();

        // Act & Assert - Retrieve and verify each category
        for (int i = 0; i < testCases.Length; i++)
        {
            var (expectedName, _) = testCases[i];
            var categoryId = categoryIds[i];

            var response = await GetApiResponseAsync<CategoryDto>($"v1/categories/{categoryId}");
            AssertApiSuccess(response);

            response!.Data.Should().NotBeNull();
            response.Data.Id.Should().Be(categoryId);
            response.Data.Name.Should().Be(expectedName);
            response.Data.IsActive.Should().BeTrue();
        }
    }

    #endregion

    // Response DTO for CreateCategory endpoint (used for test setup)
    public class CreateCategoryResponseV1
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public int Level { get; set; }
        public string Path { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
