using Moq;
using Shopilent.Application.Features.Catalog.Queries.GetAllCategories.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Results;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Catalog.Queries;

public class GetAllCategoriesQueryHandlerTests : TestBase
{
    private readonly GetAllCategoriesQueryHandlerV1 _handler;

    public GetAllCategoriesQueryHandlerTests()
    {
        _handler = new GetAllCategoriesQueryHandlerV1(
            Fixture.MockUnitOfWork.Object,
            Fixture.GetLogger<GetAllCategoriesQueryHandlerV1>());
    }

    [Fact]
    public async Task Handle_WithExistingCategories_ReturnsAllCategories()
    {
        // Arrange
        var query = new GetAllCategoriesQueryV1();

        // Create categories of mixed types (root, child, active/inactive)
        var categories = new List<CategoryDto>
        {
            new CategoryDto
            {
                Id = Guid.NewGuid(),
                Name = "Root Category 1",
                Slug = "root-category-1",
                ParentId = null,
                Level = 0,
                Path = "/root-category-1",
                IsActive = true
            },
            new CategoryDto
            {
                Id = Guid.NewGuid(),
                Name = "Root Category 2",
                Slug = "root-category-2",
                ParentId = null,
                Level = 0,
                Path = "/root-category-2",
                IsActive = false
            },
            new CategoryDto
            {
                Id = Guid.NewGuid(),
                Name = "Child Category 1",
                Slug = "child-category-1",
                ParentId = Guid.NewGuid(),
                Level = 1,
                Path = "/parent-category/child-category-1",
                IsActive = true
            }
        };

        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.ListAllAsync(CancellationToken))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.Handle(query, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);
        Assert.Contains(result.Value, c => c.Name == "Root Category 1");
        Assert.Contains(result.Value, c => c.Name == "Root Category 2");
        Assert.Contains(result.Value, c => c.Name == "Child Category 1");
    }

    [Fact]
    public async Task Handle_WithNoCategories_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllCategoriesQueryV1();

        // Mock empty categories list
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.ListAllAsync(CancellationToken))
            .ReturnsAsync(new List<CategoryDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ReturnsFailureResult()
    {
        // Arrange
        var query = new GetAllCategoriesQueryV1();

        // Mock exception
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.ListAllAsync(CancellationToken))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _handler.Handle(query, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Categories.GetAllFailed", result.Error.Code);
        Assert.Contains("Test exception", result.Error.Message);
    }
    
    [Fact]
    public async Task Handle_VerifiesCacheKeyAndExpirationAreSet()
    {
        // Arrange
        var query = new GetAllCategoriesQueryV1();
        
        // Mock successful repository call
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.ListAllAsync(CancellationToken))
            .ReturnsAsync(new List<CategoryDto>());

        // Act - no need to actually check the result here
        await _handler.Handle(query, CancellationToken);

        // Assert that cache settings are properly configured
        Assert.Equal("all-categories", query.CacheKey);
        Assert.NotNull(query.Expiration);
        Assert.Equal(TimeSpan.FromMinutes(30), query.Expiration);
    }
    
    [Fact]
    public async Task Handle_VerifiesFilteringIsNotApplied()
    {
        // Arrange
        var query = new GetAllCategoriesQueryV1();
        
        // Create mixed categories
        var allCategories = new List<CategoryDto>
        {
            new CategoryDto
            {
                Id = Guid.NewGuid(),
                Name = "Active Category",
                IsActive = true
            },
            new CategoryDto
            {
                Id = Guid.NewGuid(),
                Name = "Inactive Category",
                IsActive = false
            }
        };

        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.ListAllAsync(CancellationToken))
            .ReturnsAsync(allCategories);

        // Act
        var result = await _handler.Handle(query, CancellationToken);

        // Assert - ensure both active and inactive categories are returned
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, c => c.Name == "Active Category");
        Assert.Contains(result.Value, c => c.Name == "Inactive Category");
        
        // Verify we're using ListAllAsync and not filtering by activity status
        Fixture.MockCategoryReadRepository.Verify(
            repo => repo.ListAllAsync(CancellationToken),
            Times.Once);
    }
}