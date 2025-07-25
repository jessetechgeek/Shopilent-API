using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shopilent.Application.Features.Catalog.Commands.UpdateCategoryStatus.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Application.UnitTests.Testing.Builders;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Errors;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Catalog.Commands;

public class UpdateCategoryStatusCommandTests : TestBase
{
    private readonly IMediator _mediator;
    
    public UpdateCategoryStatusCommandTests()
    {
        // Set up MediatR pipeline
        var services = new ServiceCollection();
        
        // Register handler dependencies
        services.AddTransient(sp => Fixture.MockUnitOfWork.Object);
        services.AddTransient(sp => Fixture.MockCurrentUserContext.Object);
        services.AddTransient(sp => Fixture.GetLogger<UpdateCategoryStatusCommandHandlerV1>());

        // Set up MediatR
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssemblyContaining<UpdateCategoryStatusCommandV1>();
        });
        
        // Register validator
        services.AddTransient<FluentValidation.IValidator<UpdateCategoryStatusCommandV1>, UpdateCategoryStatusCommandValidatorV1>();
        
        // Get the mediator
        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }
    
    [Fact]
    public async Task UpdateCategoryStatus_ActivatingInactiveCategory_ReturnsSuccessfulResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        
        // Create an inactive category
        var category = new CategoryBuilder()
            .WithId(categoryId)
            .WithName("Inactive Category")
            .WithSlug("inactive-category")
            .IsInactive() // Set to inactive
            .Build();
            
        var command = new UpdateCategoryStatusCommandV1
        {
            Id = categoryId,
            IsActive = true // Activate
        };
        
        // Mock category retrieval
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.GetByIdAsync(categoryId, CancellationToken))
            .ReturnsAsync(category);
            
        // Setup authenticated user for audit info
        var userId = Guid.NewGuid();
        Fixture.SetAuthenticatedUser(userId);
        
        // Act
        var result = await _mediator.Send(command, CancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(category.IsActive); // Verify category was activated
        
        // Verify the category was saved
        Fixture.MockUnitOfWork.Verify(
            uow => uow.SaveEntitiesAsync(CancellationToken), 
            Times.Once);
    }
    
    [Fact]
    public async Task UpdateCategoryStatus_DeactivatingActiveCategory_ReturnsSuccessfulResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        
        // Create an active category
        var category = new CategoryBuilder()
            .WithId(categoryId)
            .WithName("Active Category")
            .WithSlug("active-category")
            .Build(); // Active by default
            
        var command = new UpdateCategoryStatusCommandV1
        {
            Id = categoryId,
            IsActive = false // Deactivate
        };
        
        // Mock category retrieval
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.GetByIdAsync(categoryId, CancellationToken))
            .ReturnsAsync(category);
            
        // Setup authenticated user for audit info
        var userId = Guid.NewGuid();
        Fixture.SetAuthenticatedUser(userId);
        
        // Act
        var result = await _mediator.Send(command, CancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(category.IsActive); // Verify category was deactivated
        
        // Verify the category was saved
        Fixture.MockUnitOfWork.Verify(
            uow => uow.SaveEntitiesAsync(CancellationToken), 
            Times.Once);
    }
    
    [Fact]
    public async Task UpdateCategoryStatus_WithNonExistentCategory_ReturnsErrorResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        
        var command = new UpdateCategoryStatusCommandV1
        {
            Id = categoryId,
            IsActive = true
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
            uow => uow.SaveEntitiesAsync(CancellationToken), 
            Times.Never);
    }
    
    [Fact]
    public async Task UpdateCategoryStatus_WithSameStatus_StillSucceeds()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        
        // Create an active category
        var category = new CategoryBuilder()
            .WithId(categoryId)
            .WithName("Active Category")
            .WithSlug("active-category")
            .Build(); // Active by default
            
        var command = new UpdateCategoryStatusCommandV1
        {
            Id = categoryId,
            IsActive = true // Already active
        };
        
        // Mock category retrieval
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.GetByIdAsync(categoryId, CancellationToken))
            .ReturnsAsync(category);
            
        // Setup authenticated user for audit info
        var userId = Guid.NewGuid();
        Fixture.SetAuthenticatedUser(userId);
        
        // Act
        var result = await _mediator.Send(command, CancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(category.IsActive); // Still active
        
        // Verify the category was saved (even though no real change occurred)
        Fixture.MockUnitOfWork.Verify(
            uow => uow.SaveEntitiesAsync(CancellationToken), 
            Times.Once);
    }
    
    [Fact]
    public async Task UpdateCategoryStatus_WhenExceptionOccurs_ReturnsFailureResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        
        var category = new CategoryBuilder()
            .WithId(categoryId)
            .WithName("Test Category")
            .WithSlug("test-category")
            .Build();
            
        var command = new UpdateCategoryStatusCommandV1
        {
            Id = categoryId,
            IsActive = false
        };
        
        // Mock category retrieval
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.GetByIdAsync(categoryId, CancellationToken))
            .ReturnsAsync(category);
            
        // Make SaveEntitiesAsync throw an exception
        Fixture.MockUnitOfWork
            .Setup(uow => uow.SaveEntitiesAsync(CancellationToken))
            .ThrowsAsync(new Exception("Test exception"));
            
        // Act
        var result = await _mediator.Send(command, CancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category.UpdateStatusFailed", result.Error.Code);
        Assert.Contains("Test exception", result.Error.Message);
    }
}