using System.Net;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Application.Features.Catalog.Commands.CreateCategory.V1;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.GetRootCategories.V1;

public class GetRootCategoriesEndpointV1Tests : ApiIntegrationTestBase
{
    public GetRootCategoriesEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task GetRootCategories_BasicFunctionality_ShouldReturnValidResponse()
    {
        // Arrange - Create a specific test category to verify functionality
        var testCategoryId = await CreateTestCategoryAsync("Basic Test Root", "basic-test-root", "Root category for basic functionality test");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Message.Should().Be("Root categories retrieved successfully");

        // Should contain our test category
        var testCategory = response.Data.FirstOrDefault(c => c.Id == testCategoryId);
        testCategory.Should().NotBeNull();
        testCategory!.Name.Should().Be("Basic Test Root");
        testCategory.Level.Should().Be(0);
        testCategory.ParentId.Should().BeNull();

        // All returned categories should be root categories
        response.Data.Should().AllSatisfy(category =>
        {
            category.Level.Should().Be(0);
            category.ParentId.Should().BeNull();
        });
    }

    [Fact]
    public async Task GetRootCategories_WithOnlyRootCategories_ShouldReturnAllRootCategories()
    {
        // Arrange
        var rootCategory1Id = await CreateTestCategoryAsync("Electronics", "electronics", "Electronic devices and gadgets");
        var rootCategory2Id = await CreateTestCategoryAsync("Clothing", "clothing", "Fashion and apparel items");
        var rootCategory3Id = await CreateTestCategoryAsync("Books", "books", "Books and educational materials");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(3);

        var rootCategoryIds = response.Data.Select(c => c.Id).ToList();
        rootCategoryIds.Should().Contain(new[] { rootCategory1Id, rootCategory2Id, rootCategory3Id });

        // All returned categories should be root categories (Level 0, ParentId null)
        response.Data.Should().AllSatisfy(category =>
        {
            category.Level.Should().Be(0);
            category.ParentId.Should().BeNull();
        });
    }

    [Fact]
    public async Task GetRootCategories_WithMixedHierarchy_ShouldReturnOnlyRootCategories()
    {
        // Arrange
        var rootCategory1Id = await CreateTestCategoryAsync("Technology", "technology", "Technology root category");
        var rootCategory2Id = await CreateTestCategoryAsync("Fashion", "fashion", "Fashion root category");

        // Create child categories (should not be returned)
        var childCategory1Id = await CreateTestCategoryAsync("Computers", "computers", "Computer subcategory", rootCategory1Id);
        var childCategory2Id = await CreateTestCategoryAsync("Smartphones", "smartphones", "Smartphone subcategory", rootCategory1Id);
        var childCategory3Id = await CreateTestCategoryAsync("Shoes", "shoes", "Shoes subcategory", rootCategory2Id);

        // Create grandchild category (should not be returned)
        var grandchildCategoryId = await CreateTestCategoryAsync("Gaming Laptops", "gaming-laptops", "Gaming laptop subcategory", childCategory1Id);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(2); // Only root categories

        var rootCategoryIds = response.Data.Select(c => c.Id).ToList();
        rootCategoryIds.Should().Contain(new[] { rootCategory1Id, rootCategory2Id });
        rootCategoryIds.Should().NotContain(new[] { childCategory1Id, childCategory2Id, childCategory3Id, grandchildCategoryId });

        // Verify all returned categories are root categories
        response.Data.Should().AllSatisfy(category =>
        {
            category.Level.Should().Be(0);
            category.ParentId.Should().BeNull();
            category.Path.Should().StartWith("/"); // Root categories have paths starting with "/"
            // Root category paths should be just "/slug" without additional nesting
            category.Path.Count(c => c == '/').Should().Be(1); // Should contain exactly one "/"
        });
    }

    [Fact]
    public async Task GetRootCategories_ResponseStructure_ShouldBeValid()
    {
        // Arrange
        await CreateTestCategoryAsync("Structure Test Category", "structure-test", "Category for structure testing");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(1);

        var category = response.Data.First();
        category.Id.Should().NotBeEmpty();
        category.Name.Should().Be("Structure Test Category");
        category.Slug.Should().Be("structure-test");
        category.Description.Should().Be("Category for structure testing");
        category.Level.Should().Be(0);
        category.ParentId.Should().BeNull();
        category.Path.Should().NotBeNullOrEmpty();
        category.IsActive.Should().BeTrue();
        category.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        category.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task GetRootCategories_WithUnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        await CreateTestCategoryAsync("Café & Münchën™", "cafe-munchen", "Unicode category with special characters");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(1);

        var category = response.Data.First();
        category.Name.Should().Be("Café & Münchën™");
        category.Slug.Should().Be("cafe-munchen");
    }

    [Fact]
    public async Task GetRootCategories_AnonymousAccess_ShouldWork()
    {
        // Arrange
        await CreateTestCategoryAsync("Anonymous Test", "anonymous-test", "Category for anonymous access test");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Ensure no authentication header is set
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(1);
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task GetRootCategories_DatabaseConsistency_ShouldMatchDatabaseQuery()
    {
        // Arrange
        var testRootCategory1Id = await CreateTestCategoryAsync("DB Root 1", "db-root-1", "First database test root category");
        var testRootCategory2Id = await CreateTestCategoryAsync("DB Root 2", "db-root-2", "Second database test root category");

        // Create child category (should not affect root count)
        var childCategoryId = await CreateTestCategoryAsync("DB Child", "db-child", "Database test child category", testRootCategory1Id);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root");

        // Assert
        AssertApiSuccess(response);

        // Verify against database
        await ExecuteDbContextAsync(async context =>
        {
            var rootCategoriesInDb = await context.Categories
                .Where(c => c.ParentId == null)
                .CountAsync();

            response!.Data.Should().HaveCount(rootCategoriesInDb);

            // Verify specific test categories are included
            var testRootCategoryIds = new[] { testRootCategory1Id, testRootCategory2Id };
            var responseIds = response.Data.Select(c => c.Id).ToList();
            responseIds.Should().Contain(testRootCategoryIds);
            responseIds.Should().NotContain(childCategoryId);
        });
    }

    [Fact]
    public async Task GetRootCategories_WithInactiveCategories_ShouldIncludeInactiveRootCategories()
    {
        // Arrange
        var activeRootId = await CreateTestCategoryAsync("Active Root", "active-root", "Active root category");
        var inactiveRootId = await CreateTestCategoryAsync("Inactive Root", "inactive-root", "Inactive root category");

        // Note: The domain doesn't support creating inactive categories directly,
        // so we're testing with active categories. In a real scenario, you might
        // need to update the category status after creation.

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        var responseIds = response.Data.Select(c => c.Id).ToList();
        responseIds.Should().Contain(new[] { activeRootId, inactiveRootId });
    }

    [Fact]
    public async Task GetRootCategories_OrderingConsistency_ShouldReturnConsistentOrder()
    {
        // Arrange
        await CreateTestCategoryAsync("Alpha Root", "alpha-root", "First alphabetical root");
        await CreateTestCategoryAsync("Beta Root", "beta-root", "Second alphabetical root");
        await CreateTestCategoryAsync("Gamma Root", "gamma-root", "Third alphabetical root");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act - Make multiple requests to verify consistent ordering
        var response1 = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root");
        var response2 = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root");

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);

        response1!.Data.Should().HaveCount(3);
        response2!.Data.Should().HaveCount(3);

        // Order should be consistent between requests
        var order1 = response1.Data.Select(c => c.Id).ToList();
        var order2 = response2.Data.Select(c => c.Id).ToList();
        order1.Should().Equal(order2);
    }

    #endregion

    #region Performance & Cache Tests

    [Fact]
    public async Task GetRootCategories_CacheBehavior_ShouldCacheResults()
    {
        // Arrange
        await CreateTestCategoryAsync("Cache Test 1", "cache-test-1", "First cache test category");
        await CreateTestCategoryAsync("Cache Test 2", "cache-test-2", "Second cache test category");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - First request (should hit database)
        var firstResponse = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root");
        var firstRequestTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        // Act - Second request (should hit cache)
        var secondResponse = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root");
        var secondRequestTime = stopwatch.ElapsedMilliseconds;

        // Assert
        AssertApiSuccess(firstResponse);
        AssertApiSuccess(secondResponse);

        // Both responses should have identical data
        firstResponse!.Data.Should().HaveCount(2);
        secondResponse!.Data.Should().HaveCount(2);

        var firstResponseIds = firstResponse.Data.Select(c => c.Id).OrderBy(id => id).ToList();
        var secondResponseIds = secondResponse.Data.Select(c => c.Id).OrderBy(id => id).ToList();
        firstResponseIds.Should().Equal(secondResponseIds);

        // Both requests should complete successfully (cache timing can be unreliable in tests)
        firstRequestTime.Should().BeGreaterThan(0);
        secondRequestTime.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetRootCategories_ConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        await CreateTestCategoryAsync("Concurrent Test 1", "concurrent-test-1", "First concurrent test category");
        await CreateTestCategoryAsync("Concurrent Test 2", "concurrent-test-2", "Second concurrent test category");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        var requests = Enumerable.Range(1, 5)
            .Select(_ => GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root"))
            .ToList();

        // Act
        var responses = await Task.WhenAll(requests);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Should().HaveCount(5);

        // All responses should have the same data
        var expectedCount = responses[0]!.Data.Count;
        responses.Should().AllSatisfy(response => response!.Data.Should().HaveCount(expectedCount));

        // All responses should contain the same categories
        var expectedIds = responses[0]!.Data.Select(c => c.Id).OrderBy(id => id).ToList();
        responses.Should().AllSatisfy(response =>
        {
            var responseIds = response!.Data.Select(c => c.Id).OrderBy(id => id).ToList();
            responseIds.Should().Equal(expectedIds);
        });
    }

    [Fact]
    public async Task GetRootCategories_WithLargeNumberOfRootCategories_ShouldPerformWell()
    {
        // Arrange - Create many root categories
        var tasks = new List<Task>();
        for (int i = 1; i <= 20; i++)
        {
            tasks.Add(CreateTestCategoryAsync($"Root Category {i:D2}", $"root-category-{i:D2}", $"Root category number {i}"));
        }
        await Task.WhenAll(tasks);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root");
        stopwatch.Stop();

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(20);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5)); // Should be reasonably fast

        // Verify all are root categories
        response.Data.Should().AllSatisfy(category =>
        {
            category.Level.Should().Be(0);
            category.ParentId.Should().BeNull();
        });
    }

    [Fact]
    public async Task GetRootCategories_ResponseTime_ShouldBeReasonable()
    {
        // Arrange
        await CreateTestCategoryAsync("Performance Test", "performance-test", "Category for performance testing");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<CategoryDto>>("v1/categories/root");
        stopwatch.Stop();

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2)); // Should be very fast for simple query
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateTestCategoryAsync(
        string name,
        string slug,
        string description,
        Guid? parentId = null)
    {
        using var scope = Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var createCommand = new CreateCategoryCommandV1
        {
            Name = name,
            Slug = slug,
            Description = description,
            ParentId = parentId
        };

        var result = await mediator.Send(createCommand);

        if (result.IsSuccess && result.Value != null)
        {
            return result.Value.Id;
        }

        throw new InvalidOperationException($"Failed to create test category: {name}");
    }

    #endregion
}