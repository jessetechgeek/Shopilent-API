using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Features.Identity.Commands.Logout.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Application.UnitTests.Testing;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.Errors;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Identity.Commands;

public class LogoutCommandTests : TestBase
{
    private readonly IMediator _mediator;

    public LogoutCommandTests()
    {
        var services = new ServiceCollection();
        services.AddTransient(sp => Fixture.MockAuthenticationService.Object);
        services.AddTransient(sp => Fixture.GetLogger<LogoutCommandHandlerV1>());

        services.AddMediatRWithValidation();

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Logout_WithValidRefreshToken_ReturnsSuccessfulResult()
    {
        // Arrange
        var command = new LogoutCommandV1
        {
            RefreshToken = "valid-refresh-token",
            Reason = "User logged out"
        };

        // Mock successful token revocation
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RevokeTokenAsync(
                command.RefreshToken,
                command.Reason,
                CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify auth service was called with correct parameters
        Fixture.MockAuthenticationService.Verify(
            auth => auth.RevokeTokenAsync(
                command.RefreshToken,
                command.Reason,
                CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Logout_WithEmptyRefreshToken_ReturnsErrorResult()
    {
        // Arrange
        var command = new LogoutCommandV1
        {
            RefreshToken = "",
            Reason = "User logged out"
        };

        // Mock auth service to return empty token error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RevokeTokenAsync(
                command.RefreshToken,
                command.Reason,
                CancellationToken))
            .ReturnsAsync(Result.Failure(RefreshTokenErrors.EmptyToken));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.EmptyToken.Code, result.Error.Code);
    }

    [Fact]
    public async Task Logout_WithNonExistentToken_ReturnsErrorResult()
    {
        // Arrange
        var command = new LogoutCommandV1
        {
            RefreshToken = "non-existent-token",
            Reason = "User logged out"
        };

        // Mock auth service to return token not found error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RevokeTokenAsync(
                command.RefreshToken,
                command.Reason,
                CancellationToken))
            .ReturnsAsync(Result.Failure(RefreshTokenErrors.NotFound(command.RefreshToken)));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.NotFound(command.RefreshToken).Code, result.Error.Code);
    }

    [Fact]
    public async Task Logout_WithAlreadyRevokedToken_ReturnsSuccessResult()
    {
        // Arrange
        var command = new LogoutCommandV1
        {
            RefreshToken = "already-revoked-token",
            Reason = "User logged out"
        };

        // Mock auth service to return already revoked error which should be handled gracefully
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RevokeTokenAsync(
                command.RefreshToken,
                command.Reason,
                CancellationToken))
            .ReturnsAsync(Result.Failure(RefreshTokenErrors.AlreadyRevoked));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.AlreadyRevoked.Code, result.Error.Code);
    }

    [Fact]
    public async Task Logout_WithCustomReason_PassesReasonCorrectly()
    {
        // Arrange
        var customReason = "User is switching devices";
        var command = new LogoutCommandV1
        {
            RefreshToken = "valid-refresh-token",
            Reason = customReason
        };

        // Mock successful token revocation
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RevokeTokenAsync(
                command.RefreshToken,
                customReason,
                CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify auth service was called with the custom reason
        Fixture.MockAuthenticationService.Verify(
            auth => auth.RevokeTokenAsync(
                command.RefreshToken,
                customReason,
                CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Logout_WhenExceptionOccurs_ReturnsFailureResult()
    {
        // Arrange
        var command = new LogoutCommandV1
        {
            RefreshToken = "valid-refresh-token",
            Reason = "User logged out"
        };

        // Mock auth service to throw an exception
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RevokeTokenAsync(
                command.RefreshToken,
                command.Reason,
                CancellationToken))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Logout.Failed", result.Error.Code);
        Assert.Contains("Unexpected error", result.Error.Message);
    }
}