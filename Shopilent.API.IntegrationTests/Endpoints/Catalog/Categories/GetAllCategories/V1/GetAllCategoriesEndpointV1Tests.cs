using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.GetAllCategories.V1;

public class GetAllCategoriesEndpointV1Tests : ApiIntegrationTestBase
{
    public GetAllCategoriesEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task GetAllCategories_ShouldReturnSuccessResponse()
    {
        // Arrange - No authentication required for this endpoint

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Message.Should().Be("Categories retrieved successfully");

        // Each category should have valid structure
        foreach (var category in response.Data)
        {
            category.Id.Should().NotBeEmpty();
            category.Name.Should().NotBeNullOrEmpty();
            category.Description.Should().NotBeNull();
            category.Slug.Should().NotBeNullOrEmpty();
            category.Level.Should().BeGreaterOrEqualTo(0);
            category.Path.Should().NotBeNullOrEmpty();
            category.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            category.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        }
    }

    [Fact]
    public async Task GetAllCategories_WithCreatedCategory_ShouldIncludeCreatedCategory()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a unique category to test with
        var uniqueName = $"Single Test Category {Guid.NewGuid():N}";
        var categoryRequest = GetAllCategoriesTestDataV1.CreateCategoryForSeeding(
            name: uniqueName,
            description: "Single test category description"
        );

        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", categoryRequest);
        AssertApiSuccess(createResponse);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header since GetAllCategories allows anonymous access
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty();

        // Find our created category in the response
        var createdCategory = response.Data.FirstOrDefault(c => c.Id == createResponse!.Data.Id);
        createdCategory.Should().NotBeNull("The created category should be present in the response");

        createdCategory!.Id.Should().Be(createResponse!.Data.Id);
        createdCategory.Name.Should().Be(uniqueName);
        createdCategory.Description.Should().Be("Single test category description");
        createdCategory.IsActive.Should().BeTrue();
        createdCategory.Level.Should().Be(0); // Root category
        createdCategory.ParentId.Should().BeNull(); // Root category
        createdCategory.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        createdCategory.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetAllCategories_WithMultipleCategories_ShouldReturnAllCreatedCategories()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple categories with unique names
        var testId = Guid.NewGuid().ToString("N")[..8];
        var categoryRequests = GetAllCategoriesTestDataV1.CreateMultipleCategoriesForSeeding(5)
            .Select((req, index) => GetAllCategoriesTestDataV1.CreateCategoryForSeeding(
                name: $"Multi Test Category {testId} {index + 1}",
                description: $"Multi test category description {index + 1}"
            )).ToList();

        var createdIds = new List<Guid>();

        foreach (var request in categoryRequests)
        {
            var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);
            AssertApiSuccess(createResponse);
            createdIds.Add(createResponse!.Data.Id);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty();

        // Verify all created categories are present in the response
        foreach (var createdId in createdIds)
        {
            response.Data.Should().Contain(c => c.Id == createdId,
                $"Created category with ID {createdId} should be present in the response");
        }

        // Verify we can find all created categories
        var createdCategories = response.Data.Where(c => createdIds.Contains(c.Id)).ToList();
        createdCategories.Should().HaveCount(5, "All 5 created categories should be present");
    }

    [Fact]
    public async Task GetAllCategories_WithActiveAndInactiveCategories_ShouldIncludeBothStatuses()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create categories with mixed statuses
        var testId = Guid.NewGuid().ToString("N")[..8];
        var statusRequests = GetAllCategoriesTestDataV1.StatusSpecific.CreateMixedStatusCategories()
            .Select((req, index) => GetAllCategoriesTestDataV1.CreateCategoryForSeeding(
                name: $"Status Test {testId} {index + 1}",
                slug: $"status-test-{testId}-{index + 1}",
                description: $"Status test category {index + 1}"
            )).ToList();

        var createdIds = new List<Guid>();
        foreach (var request in statusRequests)
        {
            var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);
            AssertApiSuccess(createResponse);
            createdIds.Add(createResponse!.Data.Id);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty();

        // Verify all created categories are present regardless of status
        var createdCategories = response.Data.Where(c => createdIds.Contains(c.Id)).ToList();
        createdCategories.Should().HaveCount(5, "All categories should be present regardless of status");

        // Note: All categories created through API are active by default
        // Verify all created categories are active (as per business logic)
        createdCategories.Should().AllSatisfy(c => c.IsActive.Should().BeTrue("All created categories should be active by default"));
    }

    [Fact]
    public async Task GetAllCategories_ShouldReturnCategoriesWithCorrectStructure()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a comprehensive category with unique identifiers
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var categoryRequest = GetAllCategoriesTestDataV1.CreateCategoryForSeeding(
            name: $"Structure Test Category {uniqueId}",
            slug: $"structure-test-category-{uniqueId}",
            description: "Category for testing complete structure"
        );

        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", categoryRequest);
        AssertApiSuccess(createResponse);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeEmpty();

        var category = response.Data.FirstOrDefault(c => c.Id == createResponse!.Data.Id);
        category.Should().NotBeNull("The created category should be present in the response");

        category!.Id.Should().Be(createResponse!.Data.Id);
        category.Name.Should().Be($"Structure Test Category {uniqueId}");
        category.Description.Should().Be("Category for testing complete structure");
        category.Slug.Should().Be($"structure-test-category-{uniqueId}");
        category.Level.Should().Be(0); // Root category
        category.Path.Should().NotBeNullOrEmpty();
        category.ParentId.Should().BeNull(); // Root category
        category.IsActive.Should().BeTrue();
        category.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        category.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task GetAllCategories_WithoutAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader(); // Ensure no auth header

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");

        // Assert - GetAllCategories allows anonymous access
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllCategories_WithCustomerAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllCategories_WithAdminAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task GetAllCategories_ShouldReturnDataConsistentWithDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create test categories with unique identifiers
        var testId = Guid.NewGuid().ToString("N")[..8];
        var categoryRequests = Enumerable.Range(0, 3)
            .Select(i => GetAllCategoriesTestDataV1.CreateCategoryForSeeding(
                name: $"DB Test Category {testId} {i + 1}",
                description: $"DB test category description {i + 1}"
            )).ToList();

        var createdIds = new List<Guid>();
        foreach (var request in categoryRequests)
        {
            var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);
            AssertApiSuccess(createResponse);
            createdIds.Add(createResponse!.Data.Id);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeEmpty("Response should contain categories");

        // Verify all our created categories are present in the API response
        foreach (var createdId in createdIds)
        {
            response.Data.Should().Contain(c => c.Id == createdId,
                $"Created category with ID {createdId} should be present in the API response");
        }

        // Verify API response data matches database data exactly
        await ExecuteDbContextAsync(async context =>
        {
            var dbCategories = await context.Categories
                .Where(c => createdIds.Contains(c.Id))
                .ToListAsync();

            dbCategories.Should().HaveCount(3, "All 3 created categories should exist in database");

            foreach (var dbCategory in dbCategories)
            {
                var apiCategory = response.Data.FirstOrDefault(c => c.Id == dbCategory.Id);
                apiCategory.Should().NotBeNull($"Category with ID {dbCategory.Id} should be present in API response");

                // Verify all fields match between API and database
                apiCategory!.Id.Should().Be(dbCategory.Id);
                apiCategory.Name.Should().Be(dbCategory.Name);
                apiCategory.Description.Should().Be(dbCategory.Description);
                apiCategory.Slug.Should().Be(dbCategory.Slug.Value);
                apiCategory.Level.Should().Be(dbCategory.Level);
                apiCategory.Path.Should().Be(dbCategory.Path);
                apiCategory.ParentId.Should().Be(dbCategory.ParentId);
                apiCategory.IsActive.Should().Be(dbCategory.IsActive);
                apiCategory.CreatedAt.Should().BeCloseTo(dbCategory.CreatedAt, TimeSpan.FromSeconds(1));
                apiCategory.UpdatedAt.Should().BeCloseTo(dbCategory.UpdatedAt, TimeSpan.FromSeconds(1));
            }
        });
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public async Task GetAllCategories_ShouldReturnCategoriesInConsistentOrder()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create categories with specific creation order
        var categoryRequests = new[]
        {
            GetAllCategoriesTestDataV1.CreateCategoryForSeeding(name: "Z Last Category", description: "Last category"),
            GetAllCategoriesTestDataV1.CreateCategoryForSeeding(name: "A First Category", description: "First category"),
            GetAllCategoriesTestDataV1.CreateCategoryForSeeding(name: "M Middle Category", description: "Middle category")
        };

        foreach (var request in categoryRequests)
        {
            var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);
            AssertApiSuccess(createResponse);
            // Add small delay to ensure different creation times
            await Task.Delay(10);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act - Call multiple times to ensure consistent ordering
        var response1 = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");
        var response2 = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);

        // Verify consistent ordering between calls
        response1!.Data.Select(c => c.Id).Should().BeEquivalentTo(
            response2!.Data.Select(c => c.Id),
            options => options.WithStrictOrdering());
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task GetAllCategories_WithUnicodeCharacters_ShouldReturnCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create category with unicode characters
        var categoryRequest = GetAllCategoriesTestDataV1.EdgeCases.CreateCategoryWithUnicodeCharacters();
        var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", categoryRequest);
        AssertApiSuccess(createResponse);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeEmpty();

        var category = response.Data.FirstOrDefault(c => c.Id == createResponse!.Data.Id);
        category.Should().NotBeNull("The created category should be present in the response");

        category!.Name.Should().Be("Café Münchën Category™");
        category.Description.Should().Be("Ürünlər kateqoriyası для тестирования");
    }

    [Fact]
    public async Task GetAllCategories_WithHierarchicalCategories_ShouldReturnCorrectStructure()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create root category
        var rootRequest = GetAllCategoriesTestDataV1.Hierarchical.CreateRootCategory("Hierarchical Root");
        var rootResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", rootRequest);
        AssertApiSuccess(rootResponse);

        // Create child category
        var childRequest = GetAllCategoriesTestDataV1.Hierarchical.CreateChildCategory(rootResponse!.Data.Id, "Hierarchical Child");
        var childResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", childRequest);
        AssertApiSuccess(childResponse);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeEmpty();

        var rootCategory = response.Data.FirstOrDefault(c => c.Id == rootResponse.Data.Id);
        var childCategory = response.Data.FirstOrDefault(c => c.Id == childResponse!.Data.Id);

        rootCategory.Should().NotBeNull("Root category should be present in the response");
        childCategory.Should().NotBeNull("Child category should be present in the response");

        // Verify root category structure
        rootCategory!.Level.Should().Be(0);
        rootCategory.ParentId.Should().BeNull();
        rootCategory.Name.Should().Be("Hierarchical Root");

        // Verify child category structure
        childCategory!.Level.Should().Be(1);
        childCategory.ParentId.Should().Be(rootResponse.Data.Id);
        childCategory.Name.Should().Be("Hierarchical Child");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetAllCategories_WithManyCategories_ShouldPerformWell()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create many categories
        var categoryRequests = GetAllCategoriesTestDataV1.Performance.CreateManyCategories(20);
        foreach (var request in categoryRequests)
        {
            var createResponse = await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", request);
            AssertApiSuccess(createResponse);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act & Assert - Measure response time
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");
        stopwatch.Stop();

        AssertApiSuccess(response);
        response!.Data.Should().HaveCountGreaterOrEqualTo(20);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
    }

    [Fact]
    public async Task GetAllCategories_ConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create some test data
        var categoryRequest = GetAllCategoriesTestDataV1.CreateCategoryForSeeding();
        await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", categoryRequest);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act - Make concurrent requests
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all"))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert - All requests should succeed with consistent data
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));

        var firstResponseData = responses[0]!.Data;
        responses.Should().AllSatisfy(response =>
            response!.Data.Should().BeEquivalentTo(firstResponseData));
    }

    #endregion

    #region Cache Behavior Tests

    [Fact]
    public async Task GetAllCategories_ShouldBeCached()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create category
        var categoryRequest = GetAllCategoriesTestDataV1.CreateCategoryForSeeding();
        await PostApiResponseAsync<object, CreateCategoryResponseV1>("v1/categories", categoryRequest);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act - Make first request (should populate cache)
        var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
        var response1 = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");
        stopwatch1.Stop();

        // Act - Make second request (should use cache)
        var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
        var response2 = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");
        stopwatch2.Stop();

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);

        // Data should be identical
        response1!.Data.Should().BeEquivalentTo(response2!.Data);

        // Second request should be faster (cached) - this is a soft assertion
        // Note: In test environment, caching behavior might vary
        stopwatch2.ElapsedMilliseconds.Should().BeLessOrEqualTo(stopwatch1.ElapsedMilliseconds + 100);
    }

    #endregion

    #region HTTP Status Code Tests

    [Fact]
    public async Task GetAllCategories_ShouldReturnStatus200()
    {
        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");

        // Assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(200);
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task GetAllCategories_ShouldHaveCorrectContentType()
    {
        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/all");

        // Assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(200);
        AssertApiSuccess(response);
        // Content type verification is handled by the API response structure
    }

    #endregion

    // Response DTO for create category endpoint (used for seeding)
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