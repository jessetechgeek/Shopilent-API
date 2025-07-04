// using Moq;
// using Shopilent.Application.Features.Catalog.Queries.GetPaginatedCategories.V1;
// using Shopilent.Application.UnitTests.Common;
// using Shopilent.Domain.Catalog.DTOs;
// using Shopilent.Domain.Common.Models;
// using Shopilent.Domain.Common.Results;
// using Xunit;
//
// namespace Shopilent.Application.UnitTests.Features.Catalog.Queries;
//
// public class GetPaginatedCategoriesQueryHandlerTests : TestBase
// {
//     private readonly GetPaginatedCategoriesQueryHandlerV1 _handler;
//
//     public GetPaginatedCategoriesQueryHandlerTests()
//     {
//         _handler = new GetPaginatedCategoriesQueryHandlerV1(
//             Fixture.MockUnitOfWork.Object,
//             Fixture.GetLogger<GetPaginatedCategoriesQueryHandlerV1>());
//     }
//
//     [Fact]
//     public async Task Handle_WithDefaultParameters_ReturnsPaginatedCategories()
//     {
//         // Arrange
//         var query = new GetPaginatedCategoriesQueryV1
//         {
//             // Default values:
//             // PageNumber = 1
//             // PageSize = 10
//             // SortColumn = "Name"
//             // SortDescending = false
//         };
//
//         // Create a paginated result with some categories
//         var categories = new List<CategoryDto>
//         {
//             new CategoryDto
//             {
//                 Id = Guid.NewGuid(),
//                 Name = "Category A",
//                 Slug = "category-a",
//                 Level = 0
//             },
//             new CategoryDto
//             {
//                 Id = Guid.NewGuid(),
//                 Name = "Category B",
//                 Slug = "category-b",
//                 Level = 0
//             }
//         };
//
//         var paginatedResult = new PaginatedResult<CategoryDto>(
//             pageNumber: 1,
//             pageSize: 10,
//             totalCount: 2,
//             totalPages: 1,
//             items: categories
//         );
//
//         Fixture.MockCategoryReadRepository
//             .Setup(repo => repo.GetPaginatedAsync(
//                 1, 10, "Name", false, CancellationToken))
//             .ReturnsAsync(paginatedResult);
//
//         // Act
//         var result = await _handler.Handle(query, CancellationToken);
//
//         // Assert
//         Assert.True(result.IsSuccess);
//         Assert.Equal(1, result.Value.PageNumber);
//         Assert.Equal(10, result.Value.PageSize);
//         Assert.Equal(2, result.Value.TotalCount);
//         Assert.Equal(1, result.Value.TotalPages);
//         Assert.Equal(2, result.Value.Items.Count);
//         Assert.Contains(result.Value.Items, c => c.Name == "Category A");
//         Assert.Contains(result.Value.Items, c => c.Name == "Category B");
//     }
//
//     [Fact]
//     public async Task Handle_WithCustomParameters_ReturnsPaginatedCategories()
//     {
//         // Arrange
//         var query = new GetPaginatedCategoriesQueryV1
//         {
//             PageNumber = 2,
//             PageSize = 5,
//             SortColumn = "Slug",
//             SortDescending = true
//         };
//
//         // Create a paginated result with some categories
//         var categories = new List<CategoryDto>
//         {
//             new CategoryDto
//             {
//                 Id = Guid.NewGuid(),
//                 Name = "Category C",
//                 Slug = "category-c",
//                 Level = 0
//             },
//             new CategoryDto
//             {
//                 Id = Guid.NewGuid(),
//                 Name = "Category D",
//                 Slug = "category-d",
//                 Level = 0
//             }
//         };
//
//         var paginatedResult = new PaginatedResult<CategoryDto>(
//             pageNumber: 2,
//             pageSize: 5,
//             totalCount: 10,
//             totalPages: 2,
//             items: categories
//         );
//
//         Fixture.MockCategoryReadRepository
//             .Setup(repo => repo.GetPaginatedAsync(
//                 2, 5, "Slug", true, CancellationToken))
//             .ReturnsAsync(paginatedResult);
//
//         // Act
//         var result = await _handler.Handle(query, CancellationToken);
//
//         // Assert
//         Assert.True(result.IsSuccess);
//         Assert.Equal(2, result.Value.PageNumber);
//         Assert.Equal(5, result.Value.PageSize);
//         Assert.Equal(10, result.Value.TotalCount);
//         Assert.Equal(2, result.Value.TotalPages);
//         Assert.Equal(2, result.Value.Items.Count);
//         
//         // Verify parameters were passed correctly
//         Fixture.MockCategoryReadRepository.Verify(
//             repo => repo.GetPaginatedAsync(
//                 2, 5, "Slug", true, CancellationToken),
//             Times.Once);
//     }
//
//     [Fact]
//     public async Task Handle_WithEmptyPage_ReturnsEmptyItemsList()
//     {
//         // Arrange
//         var query = new GetPaginatedCategoriesQueryV1
//         {
//             PageNumber = 3, // Page beyond available data
//             PageSize = 10
//         };
//
//         // Create an empty paginated result
//         var paginatedResult = new PaginatedResult<CategoryDto>(
//             pageNumber: 3,
//             pageSize: 10,
//             totalCount: 15, // 15 total items, but none on page 3 with pageSize 10
//             totalPages: 2,
//             items: new List<CategoryDto>() // Empty items
//         );
//
//         Fixture.MockCategoryReadRepository
//             .Setup(repo => repo.GetPaginatedAsync(
//                 3, 10, "Name", false, CancellationToken))
//             .ReturnsAsync(paginatedResult);
//
//         // Act
//         var result = await _handler.Handle(query, CancellationToken);
//
//         // Assert
//         Assert.True(result.IsSuccess);
//         Assert.Equal(3, result.Value.PageNumber);
//         Assert.Equal(10, result.Value.PageSize);
//         Assert.Equal(15, result.Value.TotalCount);
//         Assert.Equal(2, result.Value.TotalPages);
//         Assert.Empty(result.Value.Items);
//     }
//
//     [Fact]
//     public async Task Handle_WhenExceptionOccurs_ReturnsFailureResult()
//     {
//         // Arrange
//         var query = new GetPaginatedCategoriesQueryV1();
//
//         // Mock exception
//         Fixture.MockCategoryReadRepository
//             .Setup(repo => repo.GetPaginatedAsync(
//                 It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), CancellationToken))
//             .ThrowsAsync(new Exception("Test exception"));
//
//         // Act
//         var result = await _handler.Handle(query, CancellationToken);
//
//         // Assert
//         Assert.False(result.IsSuccess);
//         Assert.Equal("Categories.GetPaginatedFailed", result.Error.Code);
//         Assert.Contains("Test exception", result.Error.Message);
//     }
//     
//     [Fact]
//     public async Task Handle_VerifiesCacheKeyAndExpirationAreSet()
//     {
//         // Arrange
//         var query = new GetPaginatedCategoriesQueryV1
//         {
//             PageNumber = 2,
//             PageSize = 15,
//             SortColumn = "CreatedAt",
//             SortDescending = true
//         };
//         
//         // Create paginated result
//         var paginatedResult = new PaginatedResult<CategoryDto>(
//             pageNumber: 2,
//             pageSize: 15,
//             totalCount: 30,
//             totalPages: 2,
//             items: new List<CategoryDto>()
//         );
//         
//         // Mock successful repository call
//         Fixture.MockCategoryReadRepository
//             .Setup(repo => repo.GetPaginatedAsync(
//                 It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), CancellationToken))
//             .ReturnsAsync(paginatedResult);
//
//         // Act - no need to actually check the result here
//         await _handler.Handle(query, CancellationToken);
//
//         // Assert that cache settings are properly configured with query parameters
//         Assert.Equal($"categories-page-{query.PageNumber}-size-{query.PageSize}-sort-{query.SortColumn}-{query.SortDescending}", 
//             query.CacheKey);
//         Assert.NotNull(query.Expiration);
//         Assert.Equal(TimeSpan.FromMinutes(15), query.Expiration);
//     }
// }

using Moq;
using Shopilent.Application.Features.Catalog.Queries.GetPaginatedCategories.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Models;
using Shopilent.Domain.Common.Results;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Catalog.Queries;

public class GetPaginatedCategoriesQueryHandlerTests : TestBase
{
    private readonly GetPaginatedCategoriesQueryHandlerV1 _handler;

    public GetPaginatedCategoriesQueryHandlerTests()
    {
        _handler = new GetPaginatedCategoriesQueryHandlerV1(
            Fixture.MockUnitOfWork.Object,
            Fixture.GetLogger<GetPaginatedCategoriesQueryHandlerV1>());
    }

    [Fact]
    public async Task Handle_WithDefaultParameters_ReturnsPaginatedCategories()
    {
        // Arrange
        var query = new GetPaginatedCategoriesQueryV1
        {
            // Default values:
            // PageNumber = 1
            // PageSize = 10
            // SortColumn = "Name"
            // SortDescending = false
        };

        // Create a paginated result with some categories
        var categories = new List<CategoryDto>
        {
            new CategoryDto
            {
                Id = Guid.NewGuid(),
                Name = "Category A",
                Slug = "category-a",
                Level = 0
            },
            new CategoryDto
            {
                Id = Guid.NewGuid(),
                Name = "Category B",
                Slug = "category-b",
                Level = 0
            }
        };

        var paginatedResult = new PaginatedResult<CategoryDto>(
            items: categories,
            count: 2,
            pageNumber: 1,
            pageSize: 10
        );

        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetPaginatedAsync(
                1, 10, "Name", false, CancellationToken))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(10, result.Value.PageSize);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(1, result.Value.TotalPages);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Contains(result.Value.Items, c => c.Name == "Category A");
        Assert.Contains(result.Value.Items, c => c.Name == "Category B");
    }

    [Fact]
    public async Task Handle_WithCustomParameters_ReturnsPaginatedCategories()
    {
        // Arrange
        var query = new GetPaginatedCategoriesQueryV1
        {
            PageNumber = 2,
            PageSize = 5,
            SortColumn = "Slug",
            SortDescending = true
        };

        // Create a paginated result with some categories
        var categories = new List<CategoryDto>
        {
            new CategoryDto
            {
                Id = Guid.NewGuid(),
                Name = "Category C",
                Slug = "category-c",
                Level = 0
            },
            new CategoryDto
            {
                Id = Guid.NewGuid(),
                Name = "Category D",
                Slug = "category-d",
                Level = 0
            }
        };

        var paginatedResult = new PaginatedResult<CategoryDto>(
            items: categories,
            count: 10,
            pageNumber: 2,
            pageSize: 5
        );

        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetPaginatedAsync(
                2, 5, "Slug", true, CancellationToken))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.PageNumber);
        Assert.Equal(5, result.Value.PageSize);
        Assert.Equal(10, result.Value.TotalCount);
        Assert.Equal(2, result.Value.TotalPages);
        Assert.Equal(2, result.Value.Items.Count);
        
        // Verify parameters were passed correctly
        Fixture.MockCategoryReadRepository.Verify(
            repo => repo.GetPaginatedAsync(
                2, 5, "Slug", true, CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyPage_ReturnsEmptyItemsList()
    {
        // Arrange
        var query = new GetPaginatedCategoriesQueryV1
        {
            PageNumber = 3, // Page beyond available data
            PageSize = 10
        };

        // Create an empty paginated result
        var paginatedResult = new PaginatedResult<CategoryDto>(
            items: new List<CategoryDto>(), // Empty items
            count: 15, // 15 total items, but none on page 3 with pageSize 10
            pageNumber: 3,
            pageSize: 10
        );

        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetPaginatedAsync(
                3, 10, "Name", false, CancellationToken))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.PageNumber);
        Assert.Equal(10, result.Value.PageSize);
        Assert.Equal(15, result.Value.TotalCount);
        Assert.Equal(2, result.Value.TotalPages);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ReturnsFailureResult()
    {
        // Arrange
        var query = new GetPaginatedCategoriesQueryV1();

        // Mock exception
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetPaginatedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), CancellationToken))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _handler.Handle(query, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Categories.GetPaginatedFailed", result.Error.Code);
        Assert.Contains("Test exception", result.Error.Message);
    }
    
    [Fact]
    public async Task Handle_VerifiesCacheKeyAndExpirationAreSet()
    {
        // Arrange
        var query = new GetPaginatedCategoriesQueryV1
        {
            PageNumber = 2,
            PageSize = 15,
            SortColumn = "CreatedAt",
            SortDescending = true
        };
        
        // Create paginated result
        var paginatedResult = new PaginatedResult<CategoryDto>(
            items: new List<CategoryDto>(),
            count: 30,
            pageNumber: 2,
            pageSize: 15
        );
        
        // Mock successful repository call
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetPaginatedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), CancellationToken))
            .ReturnsAsync(paginatedResult);

        // Act - no need to actually check the result here
        await _handler.Handle(query, CancellationToken);

        // Assert that cache settings are properly configured with query parameters
        Assert.Equal($"categories-page-{query.PageNumber}-size-{query.PageSize}-sort-{query.SortColumn}-{query.SortDescending}", 
            query.CacheKey);
        Assert.NotNull(query.Expiration);
        Assert.Equal(TimeSpan.FromMinutes(15), query.Expiration);
    }
}