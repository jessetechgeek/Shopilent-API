using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.CreateCategory.V1;

public class CreateCategoryEndpointV1Tests : ApiIntegrationTestBase
{
    public CreateCategoryEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateCategory_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Creation.CreateValidRequest(
            name: "Electronics",
            slug: "electronics",
            description: "Electronic devices and gadgets");

        // Act
        var response = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().NotBeEmpty();
        response.Data.Name.Should().Be("Electronics");
        response.Data.Slug.Should().Be("electronics");
        response.Data.Description.Should().Be("Electronic devices and gadgets");
        response.Data.ParentId.Should().BeNull();
        response.Data.Level.Should().Be(0);
        response.Data.Path.Should().NotBeNullOrEmpty();
        response.Data.IsActive.Should().BeTrue();
        response.Data.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreateCategory_WithValidData_ShouldCreateCategoryInDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Creation.CreateValidRequest(
            name: "Test Category",
            slug: "test-category",
            description: "Test category description");

        // Act
        var response = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);

        // Assert
        AssertApiSuccess(response);

        // Verify category exists in database
        await ExecuteDbContextAsync(async context =>
        {
            var category = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == response!.Data.Id);

            category.Should().NotBeNull();
            category!.Name.Should().Be("Test Category");
            category.Slug.Value.Should().Be("test-category");
            category.Description.Should().Be("Test category description");
            category.ParentId.Should().BeNull();
            category.Level.Should().Be(0);
            category.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public async Task CreateCategory_WithParentCategory_ShouldReturnSuccessWithCorrectHierarchy()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create parent category first
        var parentRequest = CategoryTestDataV1.Creation.CreateValidRequest(
            name: "Parent Category",
            slug: "parent-category");
        var parentResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", parentRequest);
        AssertApiSuccess(parentResponse);

        // Create child category
        var childRequest = CategoryTestDataV1.Creation.CreateValidRequest(
            name: "Child Category",
            slug: "child-category",
            parentId: parentResponse!.Data.Id);

        // Act
        var response = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", childRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.ParentId.Should().Be(parentResponse.Data.Id);
        response.Data.Level.Should().Be(1);
        response.Data.Path.Should().Be("/parent-category/child-category");
    }

    [Fact]
    public async Task CreateCategory_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Validation.CreateRequestWithEmptyName();

        // Act
        var response = await PostAsync("v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Category name is required");
    }

    [Fact]
    public async Task CreateCategory_WithNullName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Validation.CreateRequestWithNullName();

        // Act
        var response = await PostAsync("v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Category name is required");
    }

    [Fact]
    public async Task CreateCategory_WithEmptySlug_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Validation.CreateRequestWithEmptySlug();

        // Act
        var response = await PostAsync("v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Category slug is required");
    }

    [Fact]
    public async Task CreateCategory_WithInvalidSlug_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Validation.CreateRequestWithInvalidSlug();

        // Act
        var response = await PostAsync("v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("lowercase letters, numbers, and hyphens");
    }

    [Fact]
    public async Task CreateCategory_WithUppercaseSlug_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Validation.CreateRequestWithUppercaseSlug();

        // Act
        var response = await PostAsync("v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("lowercase letters, numbers, and hyphens");
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateSlug_ShouldReturnConflict()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var slug = "duplicate-category";
        var firstRequest = CategoryTestDataV1.Creation.CreateValidRequest(
            name: "First Category",
            slug: slug);
        var secondRequest = CategoryTestDataV1.Creation.CreateValidRequest(
            name: "Second Category",
            slug: slug);

        // Act - Create first category
        var firstResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", firstRequest);
        AssertApiSuccess(firstResponse);

        // Act - Try to create second category with same slug
        var secondResponse = await PostAsync("v1/categories", secondRequest);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var content = await secondResponse.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("already exists", "duplicate", "slug");
    }

    [Fact]
    public async Task CreateCategory_WithInvalidParentId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Validation.CreateRequestWithInvalidParentId();

        // Act
        var response = await PostAsync("v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("not found", "parent", "category");
    }

    [Fact]
    public async Task CreateCategory_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = CategoryTestDataV1.Creation.CreateValidRequest();

        // Act
        var response = await PostAsync("v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCategory_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Creation.CreateValidRequest();

        // Act
        var response = await PostAsync("v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCategory_WithManagerRole_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Creation.CreateValidRequest(
            name: "Manager Category",
            slug: "manager-category");

        // Act
        var response = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("Manager Category");
    }

    // Boundary value tests
    [Fact]
    public async Task CreateCategory_WithMaximumNameLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.BoundaryTests.CreateRequestWithMaximumNameLength();

        // Act
        var response = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Length.Should().Be(100);
    }

    [Fact]
    public async Task CreateCategory_WithExcessiveNameLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Validation.CreateRequestWithLongName();

        // Act
        var response = await PostAsync("v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("100 characters");
        content.Should().Contain("name");
    }

    [Fact]
    public async Task CreateCategory_WithMaximumSlugLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.BoundaryTests.CreateRequestWithMaximumSlugLength();

        // Act
        var response = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Slug.Length.Should().Be(150);
    }

    [Fact]
    public async Task CreateCategory_WithExcessiveSlugLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Creation.CreateValidRequest(slug: new string('a', 151));

        // Act
        var response = await PostAsync("v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("150 characters");
        content.Should().Contain("slug");
    }

    [Fact]
    public async Task CreateCategory_WithMaximumDescriptionLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.BoundaryTests.CreateRequestWithMaximumDescriptionLength();

        // Act
        var response = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Description.Length.Should().Be(500);
    }

    [Fact]
    public async Task CreateCategory_WithExcessiveDescriptionLength_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Validation.CreateRequestWithLongDescription();

        // Act
        var response = await PostAsync("v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("500 characters");
        content.Should().Contain("description");
    }

    // Edge case tests
    [Fact]
    public async Task CreateCategory_WithUnicodeCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.EdgeCases.CreateRequestWithUnicodeCharactersForCreate();

        // Act
        var response = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("Café & Münchën Electronics™");
        response.Data.Description.Should().Contain("émojis 🛍️");
    }

    [Fact]
    public async Task CreateCategory_WithSpecialCharactersInName_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.EdgeCases.CreateRequestWithSpecialCharactersForCreate();

        // Act
        var response = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("Electronics & Gadgets (2024)");
    }

    [Fact]
    public async Task CreateCategory_WithMinimalData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.BoundaryTests.CreateRequestWithMinimumValidData();

        // Act
        var response = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("A");
        response.Data.Slug.Should().Be("a");
        response.Data.Description.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task CreateCategory_WithEmptyDescription_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.EdgeCases.CreateRequestWithEmptyDescription();

        // Act
        var response = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Description.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task CreateCategory_WithWhitespaceOnlyName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Validation.CreateRequestWithWhitespaceName();

        // Act
        var response = await PostAsync("v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Category name is required");
    }

    [Theory]
    [InlineData("UPPERCASE")]
    [InlineData("spaces in slug")]
    [InlineData("special@characters!")]
    [InlineData("under_scores")]
    public async Task CreateCategory_WithInvalidSlugFormats_ShouldReturnValidationError(string invalidSlug)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CategoryTestDataV1.Creation.CreateValidRequest(slug: invalidSlug);

        // Act
        var response = await PostAsync("v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("lowercase letters, numbers, and hyphens");
    }

    // Performance/Load tests
    [Fact]
    public async Task CreateCategory_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var tasks = Enumerable.Range(0, 10)
            .Select(i => CategoryTestDataV1.Creation.CreateValidRequest(
                name: $"Concurrent Category {i}",
                slug: $"concurrent-category-{i}"))
            .Select(request => PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Select(r => r!.Data.Id).Should().OnlyHaveUniqueItems();
        responses.Select(r => r!.Data.Slug).Should().OnlyHaveUniqueItems();
    }

    // Response DTO for this specific endpoint version
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
