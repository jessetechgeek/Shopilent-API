using Moq;
using Shopilent.Application.Features.Catalog.Queries.GetCategoriesDatatable.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Models;
using Shopilent.Domain.Common.Results;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Catalog.Queries;

public class GetCategoriesDatatableQueryHandlerTests : TestBase
{
    private readonly GetCategoriesDatatableQueryHandlerV1 _handler;

    public GetCategoriesDatatableQueryHandlerTests()
    {
        _handler = new GetCategoriesDatatableQueryHandlerV1(
            Fixture.MockUnitOfWork.Object,
            Fixture.GetLogger<GetCategoriesDatatableQueryHandlerV1>());
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsFormattedDatatableResult()
    {
        // Arrange
        var request = new DataTableRequest
        {
            Draw = 1,
            Start = 0,
            Length = 10,
            Search = new DataTableSearch { Value = "test" },
            Order = new List<DataTableOrder>
            {
                new DataTableOrder { Column = 0, Dir = "asc" }
            },
            Columns = new List<DataTableColumn>
            {
                new DataTableColumn { Data = "name", Name = "Name", Searchable = true, Orderable = true }
            }
        };

        var query = new GetCategoriesDatatableQueryV1
        {
            Request = request
        };

        // Create categories with parent relationships
        var parentId1 = Guid.NewGuid();
        var parentId2 = Guid.NewGuid();

        // Use CategoryDetailDto instead of CategoryDto
        var categories = new List<CategoryDetailDto>
        {
            new CategoryDetailDto
            {
                Id = Guid.NewGuid(),
                Name = "Test Category 1",
                Slug = "test-category-1",
                Description = "Test description 1",
                ParentId = parentId1,
                ParentName = "Parent Category 1", // Set directly in the DTO
                Level = 1,
                IsActive = true,
                ProductCount = 2, // Set directly in the DTO
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new CategoryDetailDto
            {
                Id = Guid.NewGuid(),
                Name = "Test Category 2",
                Slug = "test-category-2",
                Description = "Test description 2",
                ParentId = parentId2,
                ParentName = "Parent Category 2", // Set directly in the DTO
                Level = 1,
                IsActive = false,
                ProductCount = 1, // Set directly in the DTO
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        // Setup the datatable result with CategoryDetailDto
        var dataTableResult = new DataTableResult<CategoryDetailDto>(
            draw: 1,
            recordsTotal: 25,
            recordsFiltered: 2,
            data: categories
        );

        // Mock the repository call
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetCategoryDetailDataTableAsync(request, CancellationToken))
            .ReturnsAsync(dataTableResult);

        // No need to mock parent category retrieval since we're using CategoryDetailDto with ParentName already set

        // Act
        var result = await _handler.Handle(query, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify datatable metadata
        Assert.Equal(1, result.Value.Draw);
        Assert.Equal(25, result.Value.RecordsTotal);
        Assert.Equal(2, result.Value.RecordsFiltered);
        Assert.Equal(2, result.Value.Data.Count);

        // Verify the CategoryDatatableDto properties
        var firstCategory = result.Value.Data.First(c => c.Name == "Test Category 1");
        Assert.Equal(categories[0].Id, firstCategory.Id);
        Assert.Equal("test-category-1", firstCategory.Slug);
        Assert.Equal("Test description 1", firstCategory.Description);
        Assert.Equal(parentId1, firstCategory.ParentId);
        Assert.Equal("Parent Category 1", firstCategory.ParentName);
        Assert.Equal(1, firstCategory.Level);
        Assert.True(firstCategory.IsActive);
        Assert.Equal(2, firstCategory.ProductCount);

        var secondCategory = result.Value.Data.First(c => c.Name == "Test Category 2");
        Assert.Equal(categories[1].Id, secondCategory.Id);
        Assert.Equal("test-category-2", secondCategory.Slug);
        Assert.Equal("Test description 2", secondCategory.Description);
        Assert.Equal(parentId2, secondCategory.ParentId);
        Assert.Equal("Parent Category 2", secondCategory.ParentName);
        Assert.Equal(1, secondCategory.Level);
        Assert.False(secondCategory.IsActive);
        Assert.Equal(1, secondCategory.ProductCount);
    }

    [Fact]
    public async Task Handle_WithMissingParentCategories_HandlesGracefully()
    {
        // Arrange
        var request = new DataTableRequest
        {
            Draw = 1,
            Start = 0,
            Length = 10,
            Search = new DataTableSearch { Value = "" },
            Order = new List<DataTableOrder>
            {
                new DataTableOrder { Column = 0, Dir = "asc" }
            },
            Columns = new List<DataTableColumn>
            {
                new DataTableColumn { Data = "name", Name = "Name", Searchable = true, Orderable = true }
            }
        };

        var query = new GetCategoriesDatatableQueryV1
        {
            Request = request
        };

        // Create category with non-existent parent
        var nonExistentParentId = Guid.NewGuid();
        var categories = new List<CategoryDetailDto>
        {
            new CategoryDetailDto
            {
                Id = Guid.NewGuid(),
                Name = "Orphan Category",
                Slug = "orphan-category",
                ParentId = nonExistentParentId,
                ParentName = null, // No parent name
                Level = 1,
                IsActive = true,
                ProductCount = 0 // No products
            }
        };

        var dataTableResult = new DataTableResult<CategoryDetailDto>(
            draw: 1,
            recordsTotal: 1,
            recordsFiltered: 1,
            data: categories
        );

        // Mock the repository call
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetCategoryDetailDataTableAsync(request, CancellationToken))
            .ReturnsAsync(dataTableResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Data.Count);

        var categoryDto = result.Value.Data.First();
        Assert.Equal("Orphan Category", categoryDto.Name);
        Assert.Equal(nonExistentParentId, categoryDto.ParentId);
        Assert.Null(categoryDto.ParentName); // Parent name should be null
        Assert.Equal(0, categoryDto.ProductCount); // No products
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ReturnsFailureResult()
    {
        // Arrange
        var request = new DataTableRequest
        {
            Draw = 1,
            Start = 0,
            Length = 10
        };

        var query = new GetCategoriesDatatableQueryV1
        {
            Request = request
        };

        // Mock exception
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetCategoryDetailDataTableAsync(request, CancellationToken))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _handler.Handle(query, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Categories.GetDataTableFailed", result.Error.Code);
        Assert.Contains("Test exception", result.Error.Message);
    }

    [Fact]
    public async Task Handle_WithNullDataTableRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var query = new GetCategoriesDatatableQueryV1
        {
            Request = null
        };

        // Act & Assert
        // Using await Assert.ThrowsAsync instead of just Assert.ThrowsAsync
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _handler.Handle(query, CancellationToken));
    }

    [Fact]
    public async Task Handle_WithZeroDraw_ReturnsCorrectDrawNumber()
    {
        // Arrange
        var request = new DataTableRequest
        {
            Draw = 0, // Zero draw number should be preserved
            Start = 0,
            Length = 10
        };

        var query = new GetCategoriesDatatableQueryV1
        {
            Request = request
        };

        var dataTableResult = new DataTableResult<CategoryDetailDto>(
            draw: 0,
            recordsTotal: 0,
            recordsFiltered: 0,
            data: new List<CategoryDetailDto>()
        );

        // Mock the repository call
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetCategoryDetailDataTableAsync(request, CancellationToken))
            .ReturnsAsync(dataTableResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.Draw); // Draw number should be preserved
    }
}