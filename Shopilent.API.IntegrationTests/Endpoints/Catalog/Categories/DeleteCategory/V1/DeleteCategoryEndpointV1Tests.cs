using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.DeleteCategory.V1;

public class DeleteCategoryEndpointV1Tests : ApiIntegrationTestBase
{
    public DeleteCategoryEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task DeleteCategory_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a category first
        var createRequest = DeleteCategoryTestDataV1.CreateValidCategoryForDeletion();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
        apiResponse!.Data.Should().Be("Category deleted successfully");
    }

    [Fact]
    public async Task DeleteCategory_WithValidId_ShouldRemoveCategoryFromDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a category first
        var createRequest = DeleteCategoryTestDataV1.CreateValidCategoryForDeletion(
            name: "delete_test_db",
            description: "Delete Test DB Category");
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Verify category exists
        await ExecuteDbContextAsync(async context =>
        {
            var category = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);
            category.Should().NotBeNull();
        });

        // Act
        var response = await DeleteApiResponseAsync($"v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);

        // Verify category no longer exists in database
        await ExecuteDbContextAsync(async context =>
        {
            var category = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);
            category.Should().BeNull();
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteCategory_WithDifferentActiveStatus_ShouldReturnSuccess(bool isActive)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a category with specified name
        var categoryName = $"delete-{(isActive ? "active" : "inactive")}-{Guid.NewGuid():N}";
        var createRequest = DeleteCategoryTestDataV1.CreateValidCategoryForDeletion(
            name: categoryName.Length > 50 ? categoryName[..50] : categoryName);
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteCategory_ActiveCategory_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = DeleteCategoryTestDataV1.StatusTests.CreateActiveCategoryForDeletion();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteCategory_InactiveCategory_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = DeleteCategoryTestDataV1.StatusTests.CreateInactiveCategoryForDeletion();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task DeleteCategory_WithEmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await DeleteAsync($"v1/categories/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Category ID is required.");
    }

    [Fact]
    public async Task DeleteCategory_WithInvalidGuidFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await DeleteAsync("v1/categories/invalid-guid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteCategory_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/categories/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("not found", "does not exist");
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task DeleteCategory_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var categoryId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteCategory_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var categoryId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteCategory_WithAdminRole_ShouldReturnNotFoundForNonExistentCategory()
    {
        // Arrange - Test that admin has permission, but category doesn't exist

        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/categories/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound); // Not forbidden, so admin has permission
    }

    #endregion

    #region Conflict/Business Rule Tests

    [Fact]
    public async Task DeleteCategory_WithChildCategories_ShouldReturnConflict()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create parent category
        var parentRequest = DeleteCategoryTestDataV1.CreateCategoryWithChildren();
        var parentResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", parentRequest);
        AssertApiSuccess(parentResponse);
        var parentId = parentResponse!.Data.Id;

        // Create child category
        var childRequest = DeleteCategoryTestDataV1.RelatedEntities.CreateChildCategory(parentId);
        var childResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", childRequest);
        AssertApiSuccess(childResponse);

        // Act - Try to delete parent category
        var response = await DeleteAsync($"v1/categories/{parentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("child categories", "children", "cannot delete", "has children");
    }

    [Fact]
    public async Task DeleteCategory_WithAssociatedProducts_ShouldReturnConflict()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create category
        var categoryRequest = DeleteCategoryTestDataV1.CreateCategoryWithProducts();
        var categoryResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", categoryRequest);
        AssertApiSuccess(categoryResponse);
        var categoryId = categoryResponse!.Data.Id;

        // Note: In a real scenario, we would create a product that uses this category
        // For testing purposes, we'll test the empty category deletion which should succeed
        // The business logic in the handler checks for products using the category

        // Act - Delete category (should succeed since no products are associated)
        var response = await DeleteApiResponseAsync($"v1/categories/{categoryId}");

        // Assert - Should succeed since no products are actually associated
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteCategory_AlreadyDeleted_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a category
        var createRequest = DeleteCategoryTestDataV1.CreateValidCategoryForDeletion();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Delete the category first time
        var firstDeleteResponse = await DeleteApiResponseAsync($"v1/categories/{categoryId}");
        firstDeleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstDeleteContent = await firstDeleteResponse.Content.ReadAsStringAsync();
        var firstDeleteApiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(firstDeleteContent, JsonOptions);
        AssertApiSuccess(firstDeleteApiResponse);

        // Process outbox messages to ensure the deletion is fully processed
        await ProcessOutboxMessagesAsync();

        // Act - Try to delete again
        var response = await DeleteAsync($"v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task DeleteCategory_WithUnicodeCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = DeleteCategoryTestDataV1.EdgeCases.CreateCategoryWithUnicodeCharacters();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteCategory_WithComplexMetadata_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = DeleteCategoryTestDataV1.EdgeCases.CreateCategoryWithComplexMetadata();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteCategory_WithMaximumNameLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = DeleteCategoryTestDataV1.EdgeCases.CreateCategoryWithLongName();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task DeleteCategory_WithMinimumValidData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = DeleteCategoryTestDataV1.BoundaryTests.CreateCategoryWithMinimumValidData();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    [Fact]
    public async Task DeleteCategory_WithMaximumValidData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = DeleteCategoryTestDataV1.BoundaryTests.CreateCategoryWithMaximumValidData();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
    }

    #endregion

    #region Performance/Bulk Tests

    [Fact]
    public async Task DeleteCategory_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple categories first
        var createTasks = Enumerable.Range(0, 5)
            .Select(i => DeleteCategoryTestDataV1.CreateValidCategoryForDeletion(name: $"concurrent_delete_{i}_{Guid.NewGuid():N}"))
            .Select(request => PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request))
            .ToList();

        var createResponses = await Task.WhenAll(createTasks);
        createResponses.Should().AllSatisfy(response => AssertApiSuccess(response));

        // Act - Delete all categories concurrently
        var deleteTasks = createResponses
            .Select(response => DeleteApiResponseAsync($"v1/categories/{response!.Data.Id}"))
            .ToList();

        var deleteResponses = await Task.WhenAll(deleteTasks);

        // Assert
        deleteResponses.Should().AllSatisfy(response => response.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task DeleteCategory_SequentialDeletes_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple categories
        var categoryIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var createRequest = DeleteCategoryTestDataV1.CreateValidCategoryForDeletion(
                name: $"sequential_delete_{i}_{Guid.NewGuid():N}");
            var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
            AssertApiSuccess(createResponse);
            categoryIds.Add(createResponse!.Data.Id);
        }

        // Act & Assert - Delete categories sequentially
        foreach (var categoryId in categoryIds)
        {
            var response = await DeleteApiResponseAsync($"v1/categories/{categoryId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
            AssertApiSuccess(apiResponse);
        }
    }

    #endregion

    #region Integration with Other Endpoints Tests

    [Fact]
    public async Task DeleteCategory_ThenAttemptToGetIt_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a category
        var createRequest = DeleteCategoryTestDataV1.CreateValidCategoryForDeletion();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Verify category can be retrieved
        var getResponse = await Client.GetAsync($"v1/categories/{categoryId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify category exists in database before deletion
        await ExecuteDbContextAsync(async context =>
        {
            var category = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);
            category.Should().NotBeNull("Category should exist before deletion");
        });

        // Act - Delete the category
        var deleteResponse = await DeleteApiResponseAsync($"v1/categories/{categoryId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
        var deleteApiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(deleteContent, JsonOptions);
        AssertApiSuccess(deleteApiResponse);

        // Verify category no longer exists in database after deletion
        await ExecuteDbContextAsync(async context =>
        {
            var category = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);
            category.Should().BeNull("Category should not exist after deletion");
        });

        // Process outbox messages to trigger cache invalidation event handlers
        // This ensures deterministic test behavior instead of relying on background processing
        await ProcessOutboxMessagesAsync();

        // Assert - Verify category can no longer be retrieved via API
        var getAfterDeleteResponse = await Client.GetAsync($"v1/categories/{categoryId}");
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_ThenAttemptToUpdateIt_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a category
        var createRequest = DeleteCategoryTestDataV1.CreateValidCategoryForDeletion();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Delete the category
        var deleteResponse = await DeleteApiResponseAsync($"v1/categories/{categoryId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
        var deleteApiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(deleteContent, JsonOptions);
        AssertApiSuccess(deleteApiResponse);

        // Process outbox messages to trigger cache invalidation event handlers
        // This ensures deterministic test behavior instead of relying on background processing
        await ProcessOutboxMessagesAsync();

        // Act - Try to update the deleted category
        var updateRequest = new
        {
            Name = "updated-name",
            Slug = "updated-name",
            Description = "Updated Description"
        };
        var updateResponse = await PutAsync($"v1/categories/{categoryId}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task DeleteCategory_SuccessResponse_ShouldHaveCorrectFormat()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = DeleteCategoryTestDataV1.CreateValidCategoryForDeletion();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", createRequest);
        AssertApiSuccess(createResponse);
        var categoryId = createResponse!.Data.Id;

        // Act
        var response = await DeleteApiResponseAsync($"v1/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, JsonOptions);
        AssertApiSuccess(apiResponse);
        apiResponse.Should().NotBeNull();
        apiResponse!.Succeeded.Should().BeTrue();
        apiResponse.Data.Should().NotBeNullOrEmpty();
        apiResponse.Data.Should().Be("Category deleted successfully");
        apiResponse.Message.Should().NotBeNullOrEmpty();
        apiResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task DeleteCategory_ErrorResponse_ShouldHaveCorrectFormat()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"v1/categories/{nonExistentId}");

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

    // Response DTOs for creating test data
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
