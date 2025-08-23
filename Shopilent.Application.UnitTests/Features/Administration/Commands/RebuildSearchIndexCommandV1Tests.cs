using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shopilent.Application.Abstractions.Search;
using Shopilent.Application.Features.Administration.Commands.RebuildSearchIndex.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Results;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Administration.Commands;

public class RebuildSearchIndexCommandV1Tests : TestBase
{
    private readonly IMediator _mediator;

    public RebuildSearchIndexCommandV1Tests()
    {
        var services = new ServiceCollection();

        // Register handler dependencies
        services.AddTransient(sp => Fixture.MockUnitOfWork.Object);
        services.AddTransient(sp => Fixture.MockCurrentUserContext.Object);
        services.AddTransient(sp => Fixture.MockSearchService.Object);
        services.AddTransient(sp => Fixture.GetLogger<RebuildSearchIndexCommandHandlerV1>());

        // Set up MediatR
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssemblyContaining<RebuildSearchIndexCommandV1>();
        });

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Handle_ValidRequestWithInitializeAndIndex_ReturnsSuccessfulResult()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var command = new RebuildSearchIndexCommandV1
        {
            InitializeIndexes = true,
            IndexProducts = true,
            ForceReindex = false
        };

        var productDtos = new List<ProductDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1", Slug = "product-1" },
            new() { Id = Guid.NewGuid(), Name = "Product 2", Slug = "product-2" }
        };

        var productDetailDto = new ProductDetailDto
        {
            Id = productDtos[0].Id,
            Name = productDtos[0].Name,
            Slug = productDtos[0].Slug,
            Description = "Test Product 1"
        };

        // Setup authenticated admin user
        Fixture.SetAuthenticatedUser(adminUserId, isAdmin: true);

        // Mock search service
        Fixture.MockSearchService
            .Setup(service => service.InitializeIndexesAsync(CancellationToken))
            .Returns(Task.CompletedTask);

        Fixture.MockSearchService
            .Setup(service => service.IndexProductsAsync(It.IsAny<IEnumerable<ProductSearchDocument>>(), CancellationToken))
            .ReturnsAsync(Result.Success());

        // Mock product repository
        Fixture.MockUnitOfWork.Setup(uow => uow.ProductReader.ListAllAsync(CancellationToken))
            .ReturnsAsync(productDtos);

        Fixture.MockUnitOfWork.Setup(uow => uow.ProductReader.GetDetailByIdAsync(It.IsAny<Guid>(), CancellationToken))
            .ReturnsAsync(productDetailDto);

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsSuccess.Should().BeTrue();
        result.Value.IndexesInitialized.Should().BeTrue();
        result.Value.ProductsIndexed.Should().Be(2);
        result.Value.Message.Should().Contain("indexes initialized");
        result.Value.Message.Should().Contain("products indexed");

        // Verify service calls
        Fixture.MockSearchService.Verify(
            service => service.InitializeIndexesAsync(CancellationToken),
            Times.Once);

        Fixture.MockSearchService.Verify(
            service => service.IndexProductsAsync(It.IsAny<IEnumerable<ProductSearchDocument>>(), CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_InitializeOnlyRequest_ReturnsSuccessfulResult()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var command = new RebuildSearchIndexCommandV1
        {
            InitializeIndexes = true,
            IndexProducts = false,
            ForceReindex = false
        };

        // Setup authenticated admin user
        Fixture.SetAuthenticatedUser(adminUserId, isAdmin: true);

        // Mock search service
        Fixture.MockSearchService
            .Setup(service => service.InitializeIndexesAsync(CancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsSuccess.Should().BeTrue();
        result.Value.IndexesInitialized.Should().BeTrue();
        result.Value.ProductsIndexed.Should().Be(0);
        result.Value.Message.Should().Contain("indexes initialized");
        result.Value.Message.Should().NotContain("products indexed");

        // Verify service calls
        Fixture.MockSearchService.Verify(
            service => service.InitializeIndexesAsync(CancellationToken),
            Times.Once);

        Fixture.MockSearchService.Verify(
            service => service.IndexProductsAsync(It.IsAny<IEnumerable<ProductSearchDocument>>(), CancellationToken),
            Times.Never);
    }

    [Fact]
    public async Task Handle_IndexOnlyRequest_ReturnsSuccessfulResult()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var command = new RebuildSearchIndexCommandV1
        {
            InitializeIndexes = false,
            IndexProducts = true,
            ForceReindex = true
        };

        var productDtos = new List<ProductDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1", Slug = "product-1" }
        };

        var productDetailDto = new ProductDetailDto
        {
            Id = productDtos[0].Id,
            Name = productDtos[0].Name,
            Slug = productDtos[0].Slug,
            Description = "Test Product 1"
        };

        // Setup authenticated admin user
        Fixture.SetAuthenticatedUser(adminUserId, isAdmin: true);

        // Mock search service
        Fixture.MockSearchService
            .Setup(service => service.IndexProductsAsync(It.IsAny<IEnumerable<ProductSearchDocument>>(), CancellationToken))
            .ReturnsAsync(Result.Success());

        // Mock product repository
        Fixture.MockUnitOfWork.Setup(uow => uow.ProductReader.ListAllAsync(CancellationToken))
            .ReturnsAsync(productDtos);

        Fixture.MockUnitOfWork.Setup(uow => uow.ProductReader.GetDetailByIdAsync(It.IsAny<Guid>(), CancellationToken))
            .ReturnsAsync(productDetailDto);

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsSuccess.Should().BeTrue();
        result.Value.IndexesInitialized.Should().BeFalse();
        result.Value.ProductsIndexed.Should().Be(1);
        result.Value.Message.Should().NotContain("indexes initialized");
        result.Value.Message.Should().Contain("products indexed");

        // Verify service calls
        Fixture.MockSearchService.Verify(
            service => service.InitializeIndexesAsync(CancellationToken),
            Times.Never);

        Fixture.MockSearchService.Verify(
            service => service.IndexProductsAsync(It.IsAny<IEnumerable<ProductSearchDocument>>(), CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoProductsToIndex_ReturnsSuccessfulResult()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var command = new RebuildSearchIndexCommandV1
        {
            InitializeIndexes = false,
            IndexProducts = true,
            ForceReindex = false
        };

        // Setup authenticated admin user
        Fixture.SetAuthenticatedUser(adminUserId, isAdmin: true);

        // Mock empty product list
        Fixture.MockUnitOfWork.Setup(uow => uow.ProductReader.ListAllAsync(CancellationToken))
            .ReturnsAsync(new List<ProductDto>());

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsSuccess.Should().BeTrue();
        result.Value.IndexesInitialized.Should().BeFalse();
        result.Value.ProductsIndexed.Should().Be(0);
        result.Value.Message.Should().Contain("0 products indexed");

        // Verify service calls
        Fixture.MockSearchService.Verify(
            service => service.IndexProductsAsync(It.IsAny<IEnumerable<ProductSearchDocument>>(), CancellationToken),
            Times.Never);
    }

    [Fact]
    public async Task Handle_SearchServiceInitializationFails_ReturnsFailureResult()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var command = new RebuildSearchIndexCommandV1
        {
            InitializeIndexes = true,
            IndexProducts = false,
            ForceReindex = false
        };

        // Setup authenticated admin user
        Fixture.SetAuthenticatedUser(adminUserId, isAdmin: true);

        // Mock search service to throw exception
        Fixture.MockSearchService
            .Setup(service => service.InitializeIndexesAsync(CancellationToken))
            .ThrowsAsync(new InvalidOperationException("Search service unavailable"));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Search service unavailable");

        // Verify service calls
        Fixture.MockSearchService.Verify(
            service => service.InitializeIndexesAsync(CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ProductIndexingFails_ReturnsFailureResult()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var command = new RebuildSearchIndexCommandV1
        {
            InitializeIndexes = true,
            IndexProducts = true,
            ForceReindex = false
        };

        var productDtos = new List<ProductDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1", Slug = "product-1" }
        };

        var productDetailDto = new ProductDetailDto
        {
            Id = productDtos[0].Id,
            Name = productDtos[0].Name,
            Slug = productDtos[0].Slug,
            Description = "Test Product 1"
        };

        var indexingError = Domain.Common.Errors.Error.Failure("Search.IndexingFailed", "Failed to index products");

        // Setup authenticated admin user
        Fixture.SetAuthenticatedUser(adminUserId, isAdmin: true);

        // Mock search service
        Fixture.MockSearchService
            .Setup(service => service.InitializeIndexesAsync(CancellationToken))
            .Returns(Task.CompletedTask);

        Fixture.MockSearchService
            .Setup(service => service.IndexProductsAsync(It.IsAny<IEnumerable<ProductSearchDocument>>(), CancellationToken))
            .ReturnsAsync(Result.Failure(indexingError));

        // Mock product repository
        Fixture.MockUnitOfWork.Setup(uow => uow.ProductReader.ListAllAsync(CancellationToken))
            .ReturnsAsync(productDtos);

        Fixture.MockUnitOfWork.Setup(uow => uow.ProductReader.GetDetailByIdAsync(It.IsAny<Guid>(), CancellationToken))
            .ReturnsAsync(productDetailDto);

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Failed to index products");

        // Verify service calls
        Fixture.MockSearchService.Verify(
            service => service.InitializeIndexesAsync(CancellationToken),
            Times.Once);

        Fixture.MockSearchService.Verify(
            service => service.IndexProductsAsync(It.IsAny<IEnumerable<ProductSearchDocument>>(), CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ProductRepositoryFails_ReturnsFailureResult()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var command = new RebuildSearchIndexCommandV1
        {
            InitializeIndexes = false,
            IndexProducts = true,
            ForceReindex = false
        };

        // Setup authenticated admin user
        Fixture.SetAuthenticatedUser(adminUserId, isAdmin: true);

        // Mock product repository to throw exception
        Fixture.MockUnitOfWork.Setup(uow => uow.ProductReader.ListAllAsync(CancellationToken))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Database connection failed");

        // Verify service calls
        Fixture.MockUnitOfWork.Verify(
            uow => uow.ProductReader.ListAllAsync(CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsCorrectResponseFields()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var command = new RebuildSearchIndexCommandV1
        {
            InitializeIndexes = true,
            IndexProducts = true,
            ForceReindex = false
        };

        var productDtos = new List<ProductDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1", Slug = "product-1" }
        };

        var productDetailDto = new ProductDetailDto
        {
            Id = productDtos[0].Id,
            Name = productDtos[0].Name,
            Slug = productDtos[0].Slug,
            Description = "Test Product 1"
        };

        // Setup authenticated admin user
        Fixture.SetAuthenticatedUser(adminUserId, isAdmin: true);

        // Mock search service
        Fixture.MockSearchService
            .Setup(service => service.InitializeIndexesAsync(CancellationToken))
            .Returns(Task.CompletedTask);

        Fixture.MockSearchService
            .Setup(service => service.IndexProductsAsync(It.IsAny<IEnumerable<ProductSearchDocument>>(), CancellationToken))
            .ReturnsAsync(Result.Success());

        // Mock product repository
        Fixture.MockUnitOfWork.Setup(uow => uow.ProductReader.ListAllAsync(CancellationToken))
            .ReturnsAsync(productDtos);

        Fixture.MockUnitOfWork.Setup(uow => uow.ProductReader.GetDetailByIdAsync(It.IsAny<Guid>(), CancellationToken))
            .ReturnsAsync(productDetailDto);

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsSuccess.Should().BeTrue();
        result.Value.IndexesInitialized.Should().BeTrue();
        result.Value.ProductsIndexed.Should().Be(1);
        result.Value.CompletedAt.Should().NotBe(DateTime.MinValue);
        result.Value.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        result.Value.Message.Should().NotBeNull();
        result.Value.Message.Should().NotBeEmpty();
    }
}
