using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shopilent.Application.Features.Catalog.Commands.CreateCategory.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Application.UnitTests.Testing.Builders;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Catalog.ValueObjects;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Catalog.Commands;

public class CreateCategoryCommandTests : TestBase
{
    private readonly IMediator _mediator;
    
    public CreateCategoryCommandTests()
    {
        // Set up MediatR pipeline
        var services = new ServiceCollection();
        
        // Register handler dependencies
        services.AddTransient(sp => Fixture.MockUnitOfWork.Object);
        services.AddTransient(sp => Fixture.MockCurrentUserContext.Object);
        // services.AddTransient(sp => Fixture.GetLogger<CreateCategoryCommandV1>());
        services.AddTransient(sp => Fixture.GetLogger<CreateCategoryCommandHandlerV1>());

        
        // Set up MediatR
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssemblyContaining<CreateCategoryCommandV1>();
        });
        
        // Register validator
        services.AddTransient<FluentValidation.IValidator<CreateCategoryCommandV1>, CreateCategoryCommandValidatorV1>();
        
        // Get the mediator
        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }
    
    [Fact]
    public async Task CreateCategory_WithValidData_ReturnsSuccessfulResult()
    {
        // Arrange
        var command = new CreateCategoryCommandV1
        {
            Name = "Test Category",
            Slug = "test-category",
            Description = "Test category description"
        };
        
        // Mock that slug doesn't exist
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.SlugExistsAsync(command.Slug, null, CancellationToken))
            .ReturnsAsync(false);
            
        // Setup authenticated user for audit info
        var userId = Guid.NewGuid();
        Fixture.SetAuthenticatedUser(userId);
        
        // Capture the category being added
        Category capturedCategory = null;
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Category>(), CancellationToken))
            .Callback<Category, CancellationToken>((c, _) => capturedCategory = c)
            .ReturnsAsync((Category c, CancellationToken _) => c);
        
        // Act
        var result = await _mediator.Send(command, CancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify the category was created and saved correctly
        Assert.NotNull(capturedCategory);
        Assert.Equal(command.Name, capturedCategory.Name);
        Assert.Equal(command.Slug, capturedCategory.Slug.Value);
        Assert.Equal(command.Description, capturedCategory.Description);
        Assert.True(capturedCategory.IsActive);
        
        // Verify the category was saved
        Fixture.MockUnitOfWork.Verify(
            uow => uow.SaveChangesAsync(CancellationToken), 
            Times.Once);
    }
    
    [Fact]
    public async Task CreateCategory_WithDuplicateSlug_ReturnsErrorResult()
    {
        // Arrange
        var command = new CreateCategoryCommandV1
        {
            Name = "Test Category",
            Slug = "test-category",
            Description = "Test category description"
        };
        
        // Mock that slug already exists
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.SlugExistsAsync(command.Slug, null, CancellationToken))
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
    public async Task CreateCategory_WithParentCategory_CreatesChildCategoryCorrectly()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var command = new CreateCategoryCommandV1
        {
            Name = "Child Category",
            Slug = "child-category",
            Description = "Child category description",
            ParentId = parentId
        };
        
        // Mock that slug doesn't exist
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.SlugExistsAsync(command.Slug, null, CancellationToken))
            .ReturnsAsync(false);
            
        // Create a parent category
        var parentCategory = new CategoryBuilder()
            .WithId(parentId)
            .WithName("Parent Category")
            .WithSlug("parent-category")
            .Build();
            
        // Mock parent category retrieval
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.GetByIdAsync(parentId, CancellationToken))
            .ReturnsAsync(parentCategory);
            
        // Capture the category being added
        Category capturedCategory = null;
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Category>(), CancellationToken))
            .Callback<Category, CancellationToken>((c, _) => capturedCategory = c)
            .ReturnsAsync((Category c, CancellationToken _) => c);
        
        // Act
        var result = await _mediator.Send(command, CancellationToken);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedCategory);
        Assert.Equal(command.Name, capturedCategory.Name);
        Assert.Equal(command.ParentId, capturedCategory.ParentId);
        Assert.Equal(1, capturedCategory.Level); // Level 1 because it's a child
        Assert.Equal("/parent-category/child-category", capturedCategory.Path);
    }
    
    [Fact]
    public async Task CreateCategory_WithInvalidParentId_ReturnsErrorResult()
    {
        // Arrange
        var nonExistentParentId = Guid.NewGuid();
        var command = new CreateCategoryCommandV1
        {
            Name = "Child Category",
            Slug = "child-category",
            Description = "Child category description",
            ParentId = nonExistentParentId
        };
        
        // Mock that slug doesn't exist
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.SlugExistsAsync(command.Slug, null, CancellationToken))
            .ReturnsAsync(false);
            
        // Mock that parent category doesn't exist
        Fixture.MockCategoryWriteRepository
            .Setup(repo => repo.GetByIdAsync(nonExistentParentId, CancellationToken))
            .ReturnsAsync((Category)null);
        
        // Act
        var result = await _mediator.Send(command, CancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryErrors.NotFound(nonExistentParentId).Code, result.Error.Code);
    }
}