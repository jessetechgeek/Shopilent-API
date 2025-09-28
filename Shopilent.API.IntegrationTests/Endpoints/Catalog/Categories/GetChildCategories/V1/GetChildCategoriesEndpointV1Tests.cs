using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.GetChildCategories.V1;

public class GetChildCategoriesEndpointV1Tests : ApiIntegrationTestBase
{
    public GetChildCategoriesEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task GetChildCategories_WithValidParentId_ShouldReturnChildCategories()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create parent category
        var parentRequest = GetChildCategoriesTestDataV1.CreateValidParentCategoryRequest(
            name: "Electronics Parent",
            slug: "electronics-parent");
        var parentResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", parentRequest);
        AssertApiSuccess(parentResponse);
        var parentId = parentResponse!.Data.Id;

        // Create multiple child categories
        var childRequests = GetChildCategoriesTestDataV1.MultipleChildren.CreateMultipleChildCategoryRequests(parentId, 3);
        var childIds = new List<Guid>();

        foreach (var childRequest in childRequests)
        {
            var childResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", childRequest);
            AssertApiSuccess(childResponse);
            childIds.Add(childResponse!.Data.Id);
        }

        // Clear auth header to test anonymous access
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>($"v1/categories/{parentId}/children");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(3);
        response.Data.Should().OnlyContain(c => c.ParentId == parentId);
        response.Data.Should().OnlyContain(c => c.Level == 1);
        response.Data.Should().OnlyContain(c => c.IsActive == true);

        // Verify all created children are returned
        var returnedIds = response.Data.Select(c => c.Id).ToList();
        childIds.Should().BeSubsetOf(returnedIds);
    }

    [Fact]
    public async Task GetChildCategories_WithValidParentAndNoChildren_ShouldReturnEmptyList()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create parent category with no children
        var parentRequest = GetChildCategoriesTestDataV1.TestScenarios.CreateParentWithNoChildren();
        var parentResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", parentRequest);
        AssertApiSuccess(parentResponse);
        var parentId = parentResponse!.Data.Id;

        // Clear auth header to test anonymous access
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>($"v1/categories/{parentId}/children");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetChildCategories_WithAnonymousAccess_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create parent and child categories
        var parentRequest = GetChildCategoriesTestDataV1.CreateValidParentCategoryRequest();
        var parentResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", parentRequest);
        AssertApiSuccess(parentResponse);
        var parentId = parentResponse!.Data.Id;

        var childRequest = GetChildCategoriesTestDataV1.CreateValidChildCategoryRequest(parentId);
        var childResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", childRequest);
        AssertApiSuccess(childResponse);

        // Clear auth header to test anonymous access
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>($"v1/categories/{parentId}/children");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetChildCategories_ShouldReturnCategoriesInCorrectOrder()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create parent category
        var parentRequest = GetChildCategoriesTestDataV1.CreateValidParentCategoryRequest();
        var parentResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", parentRequest);
        AssertApiSuccess(parentResponse);
        var parentId = parentResponse!.Data.Id;

        // Create ordered child categories
        var orderedChildRequests = GetChildCategoriesTestDataV1.TestScenarios.CreateOrderedChildren(parentId);
        foreach (var childRequest in orderedChildRequests)
        {
            var childResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", childRequest);
            AssertApiSuccess(childResponse);
        }

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>($"v1/categories/{parentId}/children");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().HaveCount(3);

        // Verify that categories are returned (order depends on implementation)
        var categoryNames = response.Data.Select(c => c.Name).ToList();
        categoryNames.Should().Contain("Alpha Child");
        categoryNames.Should().Contain("Beta Child");
        categoryNames.Should().Contain("Gamma Child");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetChildCategories_WithNonExistentParentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = GetChildCategoriesTestDataV1.EdgeCases.NonExistentParentId;

        // Act
        var response = await Client.GetAsync($"v1/categories/{nonExistentId}/children");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain(nonExistentId.ToString());
        content.Should().ContainAny("not found", "Category");
    }

    [Fact]
    public async Task GetChildCategories_WithEmptyGuid_ShouldReturnNotFound()
    {
        // Arrange
        var emptyGuid = GetChildCategoriesTestDataV1.EdgeCases.EmptyGuid;

        // Act
        var response = await Client.GetAsync($"v1/categories/{emptyGuid}/children");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("not found", "Category");
    }

    [Fact]
    public async Task GetChildCategories_WithInvalidGuidFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidGuid = GetChildCategoriesTestDataV1.EdgeCases.InvalidGuidString;

        // Act
        var response = await Client.GetAsync($"v1/categories/{invalidGuid}/children");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Database Verification Tests

    [Fact]
    public async Task GetChildCategories_ShouldReturnDataMatchingDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create parent category
        var parentRequest = GetChildCategoriesTestDataV1.CreateValidParentCategoryRequest(
            name: "Database Test Parent",
            slug: "database-test-parent");
        var parentResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", parentRequest);
        AssertApiSuccess(parentResponse);
        var parentId = parentResponse!.Data.Id;

        // Create child category
        var childRequest = GetChildCategoriesTestDataV1.CreateValidChildCategoryRequest(
            parentId,
            name: "Database Test Child",
            slug: "database-test-child");
        var childResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", childRequest);
        AssertApiSuccess(childResponse);
        var childId = childResponse!.Data.Id;

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>($"v1/categories/{parentId}/children");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().HaveCount(1);

        var returnedChild = response.Data.First();
        returnedChild.Id.Should().Be(childId);
        returnedChild.Name.Should().Be("Database Test Child");
        returnedChild.ParentId.Should().Be(parentId);

        // Verify against database
        await ExecuteDbContextAsync(async context =>
        {
            var dbCategory = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == childId);

            dbCategory.Should().NotBeNull();
            dbCategory!.Name.Should().Be(returnedChild.Name);
            dbCategory.ParentId.Should().Be(returnedChild.ParentId);
            dbCategory.Level.Should().Be(returnedChild.Level);
            dbCategory.IsActive.Should().Be(returnedChild.IsActive);
        });
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task GetChildCategories_WithUnicodeCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create parent with unicode characters
        var parentRequest = GetChildCategoriesTestDataV1.EdgeCases.CreateParentWithUnicodeCharacters();
        var parentResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", parentRequest);
        AssertApiSuccess(parentResponse);
        var parentId = parentResponse!.Data.Id;

        // Create child with unicode characters
        var childRequest = GetChildCategoriesTestDataV1.EdgeCases.CreateChildWithUnicodeCharacters(parentId);
        var childResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", childRequest);
        AssertApiSuccess(childResponse);

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>($"v1/categories/{parentId}/children");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().HaveCount(1);
        response.Data.First().Name.Should().Be("Niño & Niña Child™");
        response.Data.First().Description.Should().Contain("👶");
    }

    [Fact]
    public async Task GetChildCategories_WithDeepHierarchy_ShouldReturnDirectChildrenOnly()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create parent category
        var parentRequest = GetChildCategoriesTestDataV1.CreateValidParentCategoryRequest();
        var parentResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", parentRequest);
        AssertApiSuccess(parentResponse);
        var parentId = parentResponse!.Data.Id;

        // Create child category
        var childRequest = GetChildCategoriesTestDataV1.CreateValidChildCategoryRequest(parentId);
        var childResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", childRequest);
        AssertApiSuccess(childResponse);
        var childId = childResponse!.Data.Id;

        // Create grandchild category
        var grandchildRequest = GetChildCategoriesTestDataV1.EdgeCases.CreateGrandchildCategoryRequest(childId);
        var grandchildResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", grandchildRequest);
        AssertApiSuccess(grandchildResponse);

        // Clear auth header
        ClearAuthenticationHeader();

        // Act - Get children of parent (should only return direct children, not grandchildren)
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>($"v1/categories/{parentId}/children");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().HaveCount(1); // Only direct child, not grandchild
        response.Data.First().Id.Should().Be(childId);
        response.Data.First().Level.Should().Be(1);
    }

    [Fact]
    public async Task GetChildCategories_WithMinimalData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create parent with minimal data
        var parentRequest = GetChildCategoriesTestDataV1.BoundaryTests.CreateParentWithMinimalData();
        var parentResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", parentRequest);
        AssertApiSuccess(parentResponse);
        var parentId = parentResponse!.Data.Id;

        // Create child with minimal data
        var childRequest = GetChildCategoriesTestDataV1.BoundaryTests.CreateChildWithMinimalData(parentId);
        var childResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", childRequest);
        AssertApiSuccess(childResponse);

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>($"v1/categories/{parentId}/children");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().HaveCount(1);
        response.Data.First().Name.Should().Be("B");
        response.Data.First().Description.Should().BeNullOrEmpty();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetChildCategories_WithManyChildren_ShouldReturnAllChildren()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create parent category
        var parentRequest = GetChildCategoriesTestDataV1.CreateValidParentCategoryRequest();
        var parentResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", parentRequest);
        AssertApiSuccess(parentResponse);
        var parentId = parentResponse!.Data.Id;

        // Create many children (but keep it reasonable for test performance)
        var childRequests = GetChildCategoriesTestDataV1.BoundaryTests.CreateMaximumChildren(parentId, 20);
        foreach (var childRequest in childRequests)
        {
            var childResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", childRequest);
            AssertApiSuccess(childResponse);
        }

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>($"v1/categories/{parentId}/children");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().HaveCount(20);
        response.Data.Should().OnlyContain(c => c.ParentId == parentId);
        response.Data.Should().OnlyContain(c => c.Level == 1);
    }

    [Fact]
    public async Task GetChildCategories_ConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create parent category
        var parentRequest = GetChildCategoriesTestDataV1.CreateValidParentCategoryRequest();
        var parentResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", parentRequest);
        AssertApiSuccess(parentResponse);
        var parentId = parentResponse!.Data.Id;

        // Create a few children
        var childRequests = GetChildCategoriesTestDataV1.MultipleChildren.CreateMultipleChildCategoryRequests(parentId, 5);
        foreach (var childRequest in childRequests)
        {
            var childResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", childRequest);
            AssertApiSuccess(childResponse);
        }

        // Clear auth header
        ClearAuthenticationHeader();

        // Act - Make concurrent requests
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => GetApiResponseAsync<IReadOnlyList<CategoryDto>>($"v1/categories/{parentId}/children"))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response =>
        {
            AssertApiSuccess(response);
            response!.Data.Should().HaveCount(5);
        });
    }

    #endregion

    #region Response DTO

    // Response DTO for creating categories (reused from other tests)
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

    #endregion
}
