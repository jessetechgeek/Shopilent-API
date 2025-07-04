using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Features.Identity.Commands.RefreshToken.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Application.UnitTests.Testing;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.Errors;
using Shopilent.Domain.Identity.ValueObjects;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Identity.Commands;

public class RefreshTokenCommandTests : TestBase
{
    private readonly IMediator _mediator;

    public RefreshTokenCommandTests()
    {
        var services = new ServiceCollection();
        services.AddTransient(sp => Fixture.MockAuthenticationService.Object);
        services.AddTransient(sp => Fixture.GetLogger<RefreshTokenCommandHandlerV1>());

        services.AddMediatRWithValidation();

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsSuccessfulResult()
    {
        // Arrange
        var command = new RefreshTokenCommandV1
        {
            RefreshToken = "valid-refresh-token",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        // Create a proper user object
        var email = Email.Create("user@example.com").Value;
        var fullName = FullName.Create("John", "Doe").Value;
        var user = User.Create(email, "hashedPassword", fullName).Value;

        var expectedResponse = new AuthTokenResponse
        {
            User = user,
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token"
        };

        // Mock successful token refresh
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RefreshTokenAsync(
                command.RefreshToken,
                command.IpAddress,
                command.UserAgent,
                CancellationToken))
            .ReturnsAsync(Result.Success(expectedResponse));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedResponse.AccessToken, result.Value.AccessToken);
        Assert.Equal(expectedResponse.RefreshToken, result.Value.RefreshToken);
        Assert.Same(user, result.Value.User);

        // Verify auth service was called with correct parameters
        Fixture.MockAuthenticationService.Verify(
            auth => auth.RefreshTokenAsync(
                command.RefreshToken,
                command.IpAddress,
                command.UserAgent,
                CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WithEmptyToken_ReturnsErrorResult()
    {
        // Arrange
        var command = new RefreshTokenCommandV1
        {
            RefreshToken = "",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        // Mock validation failure for empty token
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RefreshTokenAsync(
                command.RefreshToken,
                command.IpAddress,
                command.UserAgent,
                CancellationToken))
            .ReturnsAsync(Result.Failure<AuthTokenResponse>(RefreshTokenErrors.EmptyToken));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.EmptyToken.Code, result.Error.Code);
    }

    [Fact]
    public async Task RefreshToken_WithNonExistentToken_ReturnsErrorResult()
    {
        // Arrange
        var nonExistentToken = "non-existent-token";
        var command = new RefreshTokenCommandV1
        {
            RefreshToken = nonExistentToken,
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        // Mock token not found
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RefreshTokenAsync(
                command.RefreshToken,
                command.IpAddress,
                command.UserAgent,
                CancellationToken))
            .ReturnsAsync(Result.Failure<AuthTokenResponse>(RefreshTokenErrors.NotFound(nonExistentToken)));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.NotFound(nonExistentToken).Code, result.Error.Code);
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ReturnsErrorResult()
    {
        // Arrange
        var command = new RefreshTokenCommandV1
        {
            RefreshToken = "expired-token",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        // Mock expired token error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RefreshTokenAsync(
                command.RefreshToken,
                command.IpAddress,
                command.UserAgent,
                CancellationToken))
            .ReturnsAsync(Result.Failure<AuthTokenResponse>(RefreshTokenErrors.Expired));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.Expired.Code, result.Error.Code);
    }

    [Fact]
    public async Task RefreshToken_WithRevokedToken_ReturnsErrorResult()
    {
        // Arrange
        var command = new RefreshTokenCommandV1
        {
            RefreshToken = "revoked-token",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        var reason = "User logged out";

        // Mock revoked token error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RefreshTokenAsync(
                command.RefreshToken,
                command.IpAddress,
                command.UserAgent,
                CancellationToken))
            .ReturnsAsync(Result.Failure<AuthTokenResponse>(RefreshTokenErrors.Revoked(reason)));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.Revoked(reason).Code, result.Error.Code);
        Assert.Contains(reason, result.Error.Message);
    }

    [Fact]
    public async Task RefreshToken_WithInactiveUser_ReturnsErrorResult()
    {
        // Arrange
        var command = new RefreshTokenCommandV1
        {
            RefreshToken = "valid-token-inactive-user",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        // Mock inactive user error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RefreshTokenAsync(
                command.RefreshToken,
                command.IpAddress,
                command.UserAgent,
                CancellationToken))
            .ReturnsAsync(Result.Failure<AuthTokenResponse>(UserErrors.AccountInactive));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.AccountInactive.Code, result.Error.Code);
    }

    [Fact]
    public async Task RefreshToken_WhenExceptionOccurs_ReturnsFailureResult()
    {
        // Arrange
        var command = new RefreshTokenCommandV1
        {
            RefreshToken = "valid-token",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        // Mock auth service to throw an exception
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RefreshTokenAsync(
                command.RefreshToken,
                command.IpAddress,
                command.UserAgent,
                CancellationToken))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("RefreshToken.Failed", result.Error.Code);
        Assert.Contains("Unexpected error", result.Error.Message);
    }

    [Theory]
    [InlineData(null, "Test User Agent")]
    [InlineData("127.0.0.1", null)]
    [InlineData(null, null)]
    public async Task RefreshToken_WithMissingIpOrUserAgent_StillSucceeds(string ipAddress, string userAgent)
    {
        // Arrange
        var command = new RefreshTokenCommandV1
        {
            RefreshToken = "valid-refresh-token",
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        // Create a proper user object
        var email = Email.Create("user@example.com").Value;
        var fullName = FullName.Create("John", "Doe").Value;
        var user = User.Create(email, "hashedPassword", fullName).Value;

        var expectedResponse = new AuthTokenResponse
        {
            User = user,
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token"
        };

        // Mock successful token refresh
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RefreshTokenAsync(
                command.RefreshToken,
                ipAddress,
                userAgent,
                CancellationToken))
            .ReturnsAsync(Result.Success(expectedResponse));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify auth service was called with the provided parameters (even if null)
        Fixture.MockAuthenticationService.Verify(
            auth => auth.RefreshTokenAsync(
                command.RefreshToken,
                ipAddress,
                userAgent,
                CancellationToken),
            Times.Once);
    }
}