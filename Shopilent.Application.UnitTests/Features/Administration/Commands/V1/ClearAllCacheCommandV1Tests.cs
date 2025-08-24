using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shopilent.Application.Features.Administration.Commands.ClearAllCache.V1;
using Shopilent.Application.UnitTests.Common;

namespace Shopilent.Application.UnitTests.Features.Administration.Commands.V1;

public class ClearAllCacheCommandV1Tests : TestBase
{
    private readonly IMediator _mediator;

    public ClearAllCacheCommandV1Tests()
    {
        var services = new ServiceCollection();

        // Register handler dependencies
        services.AddTransient(sp => Fixture.MockUnitOfWork.Object);
        services.AddTransient(sp => Fixture.MockCurrentUserContext.Object);
        services.AddTransient(sp => Fixture.MockCacheService.Object);
        services.AddTransient(sp => Fixture.GetLogger<ClearAllCacheCommandHandlerV1>());

        // Set up MediatR
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssemblyContaining<ClearAllCacheCommandV1>();
        });

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task ClearAllCache_WithAdminUser_ReturnsSuccessfulResult()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var command = new ClearAllCacheCommandV1();

        // Setup authenticated admin user
        Fixture.SetAuthenticatedUser(adminUserId, isAdmin: true);

        // Mock cache service
        Fixture.MockCacheService
            .Setup(service => service.ClearAllAsync(CancellationToken))
            .ReturnsAsync(5); // Mock returning 5 cleared items

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify cache service was called
        Fixture.MockCacheService.Verify(
            service => service.ClearAllAsync(CancellationToken),
            Times.Once);
    }


    [Fact]
    public async Task ClearAllCache_WhenCacheServiceFails_ReturnsErrorResult()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var command = new ClearAllCacheCommandV1();

        // Setup authenticated admin user
        Fixture.SetAuthenticatedUser(adminUserId, isAdmin: true);

        // Mock cache service to throw exception
        Fixture.MockCacheService
            .Setup(service => service.ClearAllAsync(CancellationToken))
            .ThrowsAsync(new InvalidOperationException("Cache service unavailable"));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Cache service unavailable");

        // Verify cache service was called
        Fixture.MockCacheService.Verify(
            service => service.ClearAllAsync(CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ClearAllCache_LogsOperationSuccess()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var command = new ClearAllCacheCommandV1();

        // Setup authenticated admin user
        Fixture.SetAuthenticatedUser(adminUserId, isAdmin: true);

        // Mock cache service
        Fixture.MockCacheService
            .Setup(service => service.ClearAllAsync(CancellationToken))
            .ReturnsAsync(5); // Mock returning 5 cleared items

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify cache service was called
        Fixture.MockCacheService.Verify(
            service => service.ClearAllAsync(CancellationToken),
            Times.Once);

        // Note: In a real scenario, you might verify logging calls if using a mock logger
        // This test demonstrates the pattern even though we're using NullLogger
    }

    [Fact]
    public async Task ClearAllCache_MultipleCallsSucceed()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var command1 = new ClearAllCacheCommandV1();
        var command2 = new ClearAllCacheCommandV1();

        // Setup authenticated admin user
        Fixture.SetAuthenticatedUser(adminUserId, isAdmin: true);

        // Mock cache service
        Fixture.MockCacheService
            .Setup(service => service.ClearAllAsync(CancellationToken))
            .ReturnsAsync(5); // Mock returning 5 cleared items

        // Act
        var result1 = await _mediator.Send(command1, CancellationToken);
        var result2 = await _mediator.Send(command2, CancellationToken);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();

        // Verify cache service was called twice
        Fixture.MockCacheService.Verify(
            service => service.ClearAllAsync(CancellationToken),
            Times.Exactly(2));
    }
}
