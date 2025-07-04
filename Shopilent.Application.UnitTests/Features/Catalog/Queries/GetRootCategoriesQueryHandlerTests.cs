using Moq;
using Shopilent.Application.Features.Catalog.Queries.GetRootCategories.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Results;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Catalog.Queries;

public class GetRootCategoriesQueryHandlerTests : TestBase
{
    private readonly GetRootCategoriesQueryHandlerV1 _handler;

    public GetRootCategoriesQueryHandlerTests()
    {
        _handler = new GetRootCategoriesQueryHandlerV1(
            Fixture.MockUnitOfWork.Object,
            Fixture.GetLogger<GetRootCategoriesQueryHandlerV1>());
    }

    [Fact]
    public async Task Handle_WithExistingRootCategories_ReturnsCategories()
    {
        // Arrange
        var query = new GetRootCategoriesQueryV1();

        // Create root categories
        var rootCategories = new List<CategoryDto>
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
                IsActive = true
            }
        };

        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetRootCategoriesAsync(CancellationToken))
            .ReturnsAsync(rootCategories);

        // Act
        var result = await _handler.Handle(query, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, c => c.Name == "Root Category 1");
        Assert.Contains(result.Value, c => c.Name == "Root Category 2");
        
        // Verify all categories are actually root categories (level 0, null parent)
        Assert.All(result.Value, c => 
        {
            Assert.Equal(0, c.Level);
            Assert.Null(c.ParentId);
        });
    }

    [Fact]
    public async Task Handle_WithNoRootCategories_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetRootCategoriesQueryV1();

        // Mock empty root categories list
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetRootCategoriesAsync(CancellationToken))
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
        var query = new GetRootCategoriesQueryV1();

        // Mock exception
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetRootCategoriesAsync(CancellationToken))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _handler.Handle(query, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Categories.GetRootCategoriesFailed", result.Error.Code);
        Assert.Contains("Test exception", result.Error.Message);
    }
    
    [Fact]
    public async Task Handle_VerifiesCacheKeyAndExpirationAreSet()
    {
        // Arrange
        var query = new GetRootCategoriesQueryV1();
        
        // Mock successful repository call
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetRootCategoriesAsync(CancellationToken))
            .ReturnsAsync(new List<CategoryDto>());

        // Act - no need to actually check the result here
        await _handler.Handle(query, CancellationToken);

        // Assert that cache settings are properly configured
        Assert.Equal("root-categories", query.CacheKey);
        Assert.NotNull(query.Expiration);
        Assert.Equal(TimeSpan.FromMinutes(30), query.Expiration);
    }
}