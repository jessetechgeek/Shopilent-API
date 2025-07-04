using Moq;
using Shopilent.Application.Features.Catalog.Queries.GetCategory.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Application.UnitTests.Testing.Builders;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Common.Results;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Catalog.Queries;

public class GetCategoryQueryHandlerTests : TestBase
{
    private readonly GetCategoryQueryHandlerV1 _handler;
    
    public GetCategoryQueryHandlerTests()
    {
        _handler = new GetCategoryQueryHandlerV1(
            Fixture.MockUnitOfWork.Object,
            Fixture.GetLogger<GetCategoryQueryHandlerV1>());
    }
    
    [Fact]
    public async Task Handle_WithValidCategoryId_ReturnsSuccessfulResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var categoryDto = new CategoryDto
        {
            Id = categoryId,
            Name = "Test Category",
            Slug = "test-category",
            Description = "Test category description",
            ParentId = null,
            Level = 0,
            Path = "/test-category",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetByIdAsync(categoryId, CancellationToken))
            .ReturnsAsync(categoryDto);
            
        var query = new GetCategoryQueryV1 { Id = categoryId };
        
        // Act
        var result = await _handler.Handle(query, CancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(categoryId, result.Value.Id);
        Assert.Equal(categoryDto.Name, result.Value.Name);
        Assert.Equal(categoryDto.Slug, result.Value.Slug);
    }
    
    [Fact]
    public async Task Handle_WithInvalidCategoryId_ReturnsNotFoundError()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetByIdAsync(categoryId, CancellationToken))
            .ReturnsAsync((CategoryDto)null);
            
        var query = new GetCategoryQueryV1 { Id = categoryId };
        
        // Act
        var result = await _handler.Handle(query, CancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryErrors.NotFound(categoryId).Code, result.Error.Code);
    }
    
    [Fact]
    public async Task Handle_WhenExceptionOccurs_ReturnsFailureResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        
        Fixture.MockCategoryReadRepository
            .Setup(repo => repo.GetByIdAsync(categoryId, CancellationToken))
            .ThrowsAsync(new Exception("Test exception"));
            
        var query = new GetCategoryQueryV1 { Id = categoryId };
        
        // Act
        var result = await _handler.Handle(query, CancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category.GetFailed", result.Error.Code);
        Assert.Contains("Test exception", result.Error.Message);
    }
}