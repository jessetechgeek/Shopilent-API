using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shopilent.Application.Features.Catalog.Commands.UpdateCategory.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Application.UnitTests.Testing.Builders;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Catalog.ValueObjects;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Catalog.Commands;

public class UpdateCategoryCommandTests : TestBase
{
    private readonly IMediator _mediator;
    
    public UpdateCategoryCommandTests()
    {
        // Set up MediatR pipeline
        var services = new ServiceCollection();
        
        // Register handler dependencies
        services.AddTransient(sp => Fixture.MockUnitOfWork.Object);
        services.AddTransient(sp => Fixture.MockCurrentUserContext.Object);
        services.AddTransient(sp => Fixture.GetLogger<UpdateCategoryCommandHandlerV1>());

        // Set up MediatR
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssemblyContaining<UpdateCategoryCommandV1>();
        });
        
        // Register validator
        services.AddTransient<FluentValidation.IValidator<UpdateCategoryCommandV1>, UpdateCategoryCommandValidatorV1>();
        
        // Get the mediator
        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }
    
    [Fact]
    public async Task UpdateCategory_WithValidData_ReturnsSuccessfulResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new CategoryBuilder()
            .WithId(categoryId)
            .WithName("Original Category")
            .WithSlug("original-category")
            .Build();
            
        var command = new UpdateCategoryCommandV1
        {
            Id = categoryId,
            Name = "Updated Category",
            Slug = "updated-category",
            Description = "Updated description"
        };
        
        // Mock category retrieval
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.GetByIdAsync(categoryId, CancellationToken))
            .ReturnsAsync(category);
            
        // Mock that slug doesn't exist for other categories
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.SlugExistsAsync(command.Slug, categoryId, CancellationToken))
            .ReturnsAsync(false);
            
        // Setup authenticated user for audit info
        var userId = Guid.NewGuid();
        Fixture.SetAuthenticatedUser(userId);
        
        // Act
        var result = await _mediator.Send(command, CancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(categoryId, result.Value.Id);
        Assert.Equal(command.Name, result.Value.Name);
        Assert.Equal(command.Slug, result.Value.Slug);
        Assert.Equal(command.Description, result.Value.Description);
        
        // Verify the category was saved
        Fixture.MockUnitOfWork.Verify(
            uow => uow.SaveChangesAsync(CancellationToken), 
            Times.Once);
    }
    
    [Fact]
    public async Task UpdateCategory_WithNonExistentCategory_ReturnsErrorResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommandV1
        {
            Id = categoryId,
            Name = "Updated Category",
            Slug = "updated-category",
            Description = "Updated description"
        };
        
        // Mock that category doesn't exist
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.GetByIdAsync(categoryId, CancellationToken))
            .ReturnsAsync((Category)null);
            
        // Act
        var result = await _mediator.Send(command, CancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryErrors.NotFound(categoryId).Code, result.Error.Code);
        
        // Verify the category was not saved
        Fixture.MockUnitOfWork.Verify(
            uow => uow.SaveChangesAsync(CancellationToken), 
            Times.Never);
    }
    
    [Fact]
    public async Task UpdateCategory_WithDuplicateSlug_ReturnsErrorResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new CategoryBuilder()
            .WithId(categoryId)
            .WithName("Original Category")
            .WithSlug("original-category")
            .Build();
            
        var command = new UpdateCategoryCommandV1
        {
            Id = categoryId,
            Name = "Updated Category",
            Slug = "duplicate-slug",
            Description = "Updated description"
        };
        
        // Mock category retrieval
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.GetByIdAsync(categoryId, CancellationToken))
            .ReturnsAsync(category);
            
        // Mock that slug exists for other categories
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.SlugExistsAsync(command.Slug, categoryId, CancellationToken))
            .ReturnsAsync(true);
            
        // Act
        var result = await _mediator.Send(command, CancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryErrors.DuplicateSlug(command.Slug).Code, result.Error.Code);
        
        // Verify the category was not saved
        Fixture.MockUnitOfWork.Verify(
            uow => uow.SaveChangesAsync(CancellationToken), 
            Times.Never);
    }
    
    [Fact]
    public async Task UpdateCategory_WithInvalidSlug_ReturnsErrorResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommandV1
        {
            Id = categoryId,
            Name = "Updated Category",
            Slug = "invalid slug with spaces",
            Description = "Updated description"
        };
            
        // Act
        var result = await _mediator.Send(command, CancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryErrors.NotFound(categoryId).Code, result.Error.Code);
        
        // Verify the category was not saved
        Fixture.MockUnitOfWork.Verify(
            uow => uow.SaveChangesAsync(CancellationToken), 
            Times.Never);
    }
    
    [Fact]
    public async Task UpdateCategory_WithIsActiveTrue_ActivatesCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new CategoryBuilder()
            .WithId(categoryId)
            .WithName("Original Category")
            .WithSlug("original-category")
            .IsInactive() // Create inactive category
            .Build();
            
        var command = new UpdateCategoryCommandV1
        {
            Id = categoryId,
            Name = "Updated Category",
            Slug = "updated-category",
            Description = "Updated description",
            IsActive = true // Set to active
        };
        
        // Mock category retrieval
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.GetByIdAsync(categoryId, CancellationToken))
            .ReturnsAsync(category);
            
        // Mock that slug doesn't exist for other categories
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.SlugExistsAsync(command.Slug, categoryId, CancellationToken))
            .ReturnsAsync(false);
        
        // Setup authenticated user for audit info
        var userId = Guid.NewGuid();
        Fixture.SetAuthenticatedUser(userId);
        
        // Act
        var result = await _mediator.Send(command, CancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsActive);
        
        // Verify the category was saved
        Fixture.MockUnitOfWork.Verify(
            uow => uow.SaveChangesAsync(CancellationToken), 
            Times.Once);
    }
    
    [Fact]
    public async Task UpdateCategory_WithIsActiveFalse_DeactivatesCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new CategoryBuilder()
            .WithId(categoryId)
            .WithName("Original Category")
            .WithSlug("original-category")
            .Build(); // Active by default
            
        var command = new UpdateCategoryCommandV1
        {
            Id = categoryId,
            Name = "Updated Category",
            Slug = "updated-category",
            Description = "Updated description",
            IsActive = false // Set to inactive
        };
        
        // Mock category retrieval
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.GetByIdAsync(categoryId, CancellationToken))
            .ReturnsAsync(category);
            
        // Mock that slug doesn't exist for other categories
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.SlugExistsAsync(command.Slug, categoryId, CancellationToken))
            .ReturnsAsync(false);
        
        // Setup authenticated user for audit info
        var userId = Guid.NewGuid();
        Fixture.SetAuthenticatedUser(userId);
        
        // Act
        var result = await _mediator.Send(command, CancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsActive);
        
        // Verify the category was saved
        Fixture.MockUnitOfWork.Verify(
            uow => uow.SaveChangesAsync(CancellationToken), 
            Times.Once);
    }
}