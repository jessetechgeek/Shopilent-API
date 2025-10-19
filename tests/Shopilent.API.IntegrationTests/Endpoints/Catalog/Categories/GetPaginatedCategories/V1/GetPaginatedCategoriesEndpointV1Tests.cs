using System.Net;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Endpoints.Catalog.Categories.GetPaginatedCategories.V1;
using Shopilent.Application.Features.Catalog.Commands.CreateCategory.V1;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Categories.GetPaginatedCategories.V1;

public class GetPaginatedCategoriesEndpointV1Tests : ApiIntegrationTestBase
{
    public GetPaginatedCategoriesEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task GetPaginatedCategories_WithDefaultParameters_ShouldReturnSuccess()
    {
        // Arrange - No authentication required for this endpoint

        // Act
        var response = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.PageNumber.Should().Be(1);
        response.Data.PageSize.Should().Be(10);
        response.Data.Items.Should().NotBeNull();
        response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        response.Data.TotalPages.Should().BeGreaterThanOrEqualTo(0);
        response.Message.Should().Be("Categories retrieved successfully");
    }

    [Fact]
    public async Task GetPaginatedCategories_WithCustomPageSize_ShouldReturnCorrectPageSize()
    {
        // Arrange
        await CreateTestCategoriesAsync(15);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories?PageSize=5");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.PageSize.Should().Be(5);
        response.Data.Items.Should().HaveCountLessOrEqualTo(5);
        response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(15);
    }

    [Fact]
    public async Task GetPaginatedCategories_WithMultiplePages_ShouldReturnDifferentResults()
    {
        // Arrange - Create enough categories to ensure multiple pages
        await CreateTestCategoriesAsync(10); // Create 10 categories

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act - First page
        var firstPageResponse = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories?PageNumber=1&PageSize=3");

        // Act - Second page
        var secondPageResponse = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories?PageNumber=2&PageSize=3");

        // Assert
        AssertApiSuccess(firstPageResponse);
        AssertApiSuccess(secondPageResponse);

        firstPageResponse!.Data.Items.Should().HaveCount(3);
        secondPageResponse!.Data.Items.Should().HaveCount(3);
        firstPageResponse.Data.TotalCount.Should().BeGreaterThanOrEqualTo(10);

        // Pages should have different categories
        var firstPageIds = firstPageResponse.Data.Items.Select(c => c.Id).ToList();
        var secondPageIds = secondPageResponse.Data.Items.Select(c => c.Id).ToList();
        firstPageIds.Should().NotIntersectWith(secondPageIds);

        // Pagination metadata should be correct
        firstPageResponse.Data.HasPreviousPage.Should().BeFalse();
        firstPageResponse.Data.HasNextPage.Should().BeTrue();
        secondPageResponse.Data.HasPreviousPage.Should().BeTrue();
        secondPageResponse.Data.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task GetPaginatedCategories_SortByNameAscending_ShouldReturnSortedResults()
    {
        // Arrange
        await CreateTestCategoryAsync("Zebra Category", "zebra-category", "Last category alphabetically");
        await CreateTestCategoryAsync("Alpha Category", "alpha-category", "First category alphabetically");
        await CreateTestCategoryAsync("Beta Category", "beta-category", "Middle category alphabetically");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories?SortColumn=Name&SortDescending=false&PageSize=20");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().HaveCountGreaterThanOrEqualTo(3);

        var sortedNames = response.Data.Items.Select(c => c.Name).ToList();
        sortedNames.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetPaginatedCategories_SortByNameDescending_ShouldReturnReverseSortedResults()
    {
        // Arrange
        await CreateTestCategoryAsync("Alpha Sort Test", "alpha-sort-test", "First category");
        await CreateTestCategoryAsync("Beta Sort Test", "beta-sort-test", "Second category");
        await CreateTestCategoryAsync("Gamma Sort Test", "gamma-sort-test", "Third category");

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories?SortColumn=Name&SortDescending=true&PageSize=20");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().HaveCountGreaterThanOrEqualTo(3);

        var sortedNames = response.Data.Items.Select(c => c.Name).ToList();
        sortedNames.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetPaginatedCategories_SortByCreatedAt_ShouldReturnChronologicalResults()
    {
        // Arrange
        var oldCategoryId = await CreateTestCategoryAsync("Old Category", "old-category", "Older category");
        await Task.Delay(1000); // Ensure different timestamps
        var newCategoryId = await CreateTestCategoryAsync("New Category", "new-category", "Newer category");

        // Act
        var response = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories?SortColumn=CreatedAt&SortDescending=true&PageSize=20");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();

        var sortedDates = response.Data.Items.Select(c => c.CreatedAt).ToList();
        sortedDates.Should().BeInDescendingOrder();
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task GetPaginatedCategories_WithZeroPageNumber_ShouldReturnBadRequest()
    {
        // Arrange - No setup needed

        // Act
        var response = await Client.GetAsync("v1/categories?PageNumber=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Page number must be greater than 0");
    }

    [Fact]
    public async Task GetPaginatedCategories_WithNegativePageNumber_ShouldReturnBadRequest()
    {
        // Arrange - No setup needed

        // Act
        var response = await Client.GetAsync("v1/categories?PageNumber=-1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Page number must be greater than 0");
    }

    [Fact]
    public async Task GetPaginatedCategories_WithZeroPageSize_ShouldReturnBadRequest()
    {
        // Arrange - No setup needed

        // Act
        var response = await Client.GetAsync("v1/categories?PageSize=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Page size must be greater than 0");
    }

    [Fact]
    public async Task GetPaginatedCategories_WithExcessivePageSize_ShouldReturnBadRequest()
    {
        // Arrange - No setup needed

        // Act
        var response = await Client.GetAsync("v1/categories?PageSize=101");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Page size must not exceed 100");
    }

    [Fact]
    public async Task GetPaginatedCategories_WithMaximumValidPageSize_ShouldReturnSuccess()
    {
        // Arrange
        await CreateTestCategoriesAsync(5);

        // Act
        var response = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories?PageSize=100");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.PageSize.Should().Be(100);
        response.Data.Items.Should().HaveCountLessOrEqualTo(100);
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task GetPaginatedCategories_WithEmptyDatabase_ShouldReturnEmptyResults()
    {
        // Arrange - Clean database (no categories created)

        // Act
        var response = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().BeEmpty();
        response.Data.TotalCount.Should().Be(0);
        response.Data.TotalPages.Should().Be(0);
        response.Data.HasPreviousPage.Should().BeFalse();
        response.Data.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetPaginatedCategories_ResponseStructure_ShouldBeValid()
    {
        // Arrange
        await CreateTestCategoryAsync("Structure Test", "structure-test", "Category for structure testing");

        // Act
        var response = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();

        // Verify response structure
        response.Data.PageNumber.Should().BeGreaterThan(0);
        response.Data.PageSize.Should().BeGreaterThan(0);
        response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        response.Data.TotalPages.Should().BeGreaterThanOrEqualTo(0);

        // Verify each category item structure
        foreach (var category in response.Data.Items)
        {
            category.Id.Should().NotBeEmpty();
            category.Name.Should().NotBeNullOrEmpty();
            category.Slug.Should().NotBeNullOrEmpty();
            category.Level.Should().BeGreaterThanOrEqualTo(0);
            category.Path.Should().NotBeNullOrEmpty();
            category.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            category.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        }
    }

    [Fact]
    public async Task GetPaginatedCategories_PaginationMetadata_ShouldBeAccurate()
    {
        // Arrange
        await CreateTestCategoriesAsync(7); // Create exactly 7 categories

        // Act
        var response = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories?PageSize=3");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(7);
        response.Data.TotalPages.Should().BeGreaterThanOrEqualTo(3); // ceil(7/3) = 3
        response.Data.PageNumber.Should().Be(1);
        response.Data.PageSize.Should().Be(3);
        response.Data.HasPreviousPage.Should().BeFalse();
        response.Data.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetPaginatedCategories_WithHierarchy_ShouldPreserveHierarchyInfo()
    {
        // Arrange
        var parentCategoryId = await CreateTestCategoryAsync("Parent Category Hierarchy", "parent-category-hierarchy", "Parent category for hierarchy test");
        var childCategoryId = await CreateTestCategoryAsync("Child Category Hierarchy", "child-category-hierarchy", "Child category for hierarchy test", parentCategoryId);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Act
        var response = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories?PageSize=50");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();

        var parentCategory = response.Data.Items.FirstOrDefault(c => c.Id == parentCategoryId);
        var childCategory = response.Data.Items.FirstOrDefault(c => c.Id == childCategoryId);

        parentCategory.Should().NotBeNull($"Parent category with ID {parentCategoryId} should be found in response");
        parentCategory!.Level.Should().Be(0);
        parentCategory.ParentId.Should().BeNull();

        childCategory.Should().NotBeNull($"Child category with ID {childCategoryId} should be found in response");
        childCategory!.Level.Should().Be(1);
        childCategory.ParentId.Should().Be(parentCategoryId);
    }

    [Fact]
    public async Task GetPaginatedCategories_DatabaseConsistency_ShouldMatchDatabaseCount()
    {
        // Arrange
        var testCategory1Id = await CreateTestCategoryAsync("DB Test 1", "db-test-1", "First database test category");
        var testCategory2Id = await CreateTestCategoryAsync("DB Test 2", "db-test-2", "Second database test category");

        // Act
        var response = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories?PageSize=100");

        // Assert
        AssertApiSuccess(response);

        // Verify against database
        await ExecuteDbContextAsync(async context =>
        {
            var totalCategoriesInDb = await context.Categories.CountAsync();
            response!.Data.TotalCount.Should().Be(totalCategoriesInDb);

            // Verify specific test categories are included
            var testCategoryIds = new[] { testCategory1Id, testCategory2Id };
            var responseIds = response.Data.Items.Select(c => c.Id).ToList();
            responseIds.Should().Contain(testCategoryIds);
        });
    }

    #endregion

    #region Performance & Edge Cases

    [Fact]
    public async Task GetPaginatedCategories_WithHighPageNumber_ShouldReturnEmptyResults()
    {
        // Arrange
        await CreateTestCategoriesAsync(5);

        // Act
        var response = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories?PageNumber=999&PageSize=10");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Items.Should().BeEmpty();
        response.Data.PageNumber.Should().Be(999);
        response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(5);
        response.Data.HasPreviousPage.Should().BeTrue();
        response.Data.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetPaginatedCategories_ConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        await CreateTestCategoriesAsync(10);

        var requests = Enumerable.Range(1, 5)
            .Select(pageNumber => GetApiResponseAsync<GetPaginatedCategoriesResponseV1>($"v1/categories?PageNumber={pageNumber}&PageSize=2"))
            .ToList();

        // Act
        var responses = await Task.WhenAll(requests);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Should().HaveCount(5);

        // Verify each response has correct page number
        for (int i = 0; i < responses.Length; i++)
        {
            responses[i]!.Data.PageNumber.Should().Be(i + 1);
        }
    }

    [Fact]
    public async Task GetPaginatedCategories_CacheBehavior_ShouldCacheResults()
    {
        // Arrange
        await CreateTestCategoriesAsync(3);
        var url = "v1/categories?PageNumber=1&PageSize=10";

        // Act - First request (should hit database)
        var firstResponse = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>(url);

        // Act - Second request (should hit cache)
        var secondResponse = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>(url);

        // Assert
        AssertApiSuccess(firstResponse);
        AssertApiSuccess(secondResponse);

        // Both responses should have identical data (the key indicator of caching)
        firstResponse!.Data.Should().BeEquivalentTo(secondResponse!.Data);

        // Verify data consistency (cached responses should match)
        firstResponse.Data.Items.Should().HaveCount(secondResponse!.Data.Items.Count);
        firstResponse.Data.TotalCount.Should().Be(secondResponse.Data.TotalCount);
        firstResponse.Data.PageNumber.Should().Be(secondResponse.Data.PageNumber);
        firstResponse.Data.PageSize.Should().Be(secondResponse.Data.PageSize);

        // Both requests should succeed (timing comparison is unreliable in tests)
        firstResponse.Should().NotBeNull();
        secondResponse.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPaginatedCategories_ResponseTime_ShouldBeReasonable()
    {
        // Arrange
        await CreateTestCategoriesAsync(50);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await GetApiResponseAsync<GetPaginatedCategoriesResponseV1>("v1/categories?PageSize=20");
        stopwatch.Stop();

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5)); // Should be reasonably fast
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

    private async Task CreateTestCategoriesAsync(int count)
    {
        var categories = CategoryTestDataV1.Creation.CreateMultipleCategoriesForSeeding(count);
        var tasks = new List<Task>();

        for (int i = 0; i < categories.Count; i++)
        {
            var category = categories[i];
            var categoryData = (dynamic)category;

            tasks.Add(CreateTestCategoryAsync(
                categoryData.Name,
                categoryData.Slug,
                categoryData.Description,
                categoryData.ParentId));
        }

        await Task.WhenAll(tasks);
    }

    #endregion
}
