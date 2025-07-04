using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Features.Identity.Commands.VerifyEmail.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Application.UnitTests.Testing;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Identity.Commands;

public class VerifyEmailCommandTests : TestBase
{
    private readonly IMediator _mediator;

    public VerifyEmailCommandTests()
    {
        var services = new ServiceCollection();
        services.AddTransient(sp => Fixture.MockAuthenticationService.Object);
        services.AddTransient(sp => Fixture.GetLogger<VerifyEmailCommandHandlerV1>());

        services.AddMediatRWithValidation();

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task VerifyEmail_WithValidToken_ReturnsSuccessfulResult()
    {
        // Arrange
        var command = new VerifyEmailCommandV1
        {
            Token = "valid-verification-token"
        };

        // Mock successful email verification
        Fixture.MockAuthenticationService
            .Setup(auth => auth.VerifyEmailAsync(
                command.Token,
                CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify auth service was called with correct token
        Fixture.MockAuthenticationService.Verify(
            auth => auth.VerifyEmailAsync(
                command.Token,
                CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task VerifyEmail_WithEmptyToken_ReturnsValidationError()
    {
        // Arrange
        var command = new VerifyEmailCommandV1
        {
            Token = ""
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation.Failed", result.Error.Code);

        // Verify auth service was not called
        Fixture.MockAuthenticationService.Verify(
            auth => auth.VerifyEmailAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task VerifyEmail_WithInvalidToken_ReturnsErrorResult()
    {
        // Arrange
        var command = new VerifyEmailCommandV1
        {
            Token = "invalid-verification-token"
        };

        // Mock invalid token error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.VerifyEmailAsync(
                command.Token,
                CancellationToken))
            .ReturnsAsync(Result.Failure(Error.Validation(
                code: "EmailVerification.InvalidToken",
                message: "The verification token is invalid.")));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("EmailVerification.InvalidToken", result.Error.Code);
    }

    [Fact]
    public async Task VerifyEmail_WithExpiredToken_ReturnsErrorResult()
    {
        // Arrange
        var command = new VerifyEmailCommandV1
        {
            Token = "expired-verification-token"
        };

        // Mock expired token error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.VerifyEmailAsync(
                command.Token,
                CancellationToken))
            .ReturnsAsync(Result.Failure(Error.Validation(
                code: "EmailVerification.ExpiredToken",
                message: "The verification token has expired.")));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("EmailVerification.ExpiredToken", result.Error.Code);
    }

    [Fact]
    public async Task VerifyEmail_ForAlreadyVerifiedEmail_ReturnsSuccessResult()
    {
        // Arrange
        var command = new VerifyEmailCommandV1
        {
            Token = "token-for-verified-email"
        };

        // Mock already verified success (the service should handle this gracefully)
        Fixture.MockAuthenticationService
            .Setup(auth => auth.VerifyEmailAsync(
                command.Token,
                CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task VerifyEmail_WhenExceptionOccurs_ReturnsFailureResult()
    {
        // Arrange
        var command = new VerifyEmailCommandV1
        {
            Token = "valid-verification-token"
        };

        // Mock auth service to throw an exception
        Fixture.MockAuthenticationService
            .Setup(auth => auth.VerifyEmailAsync(
                command.Token,
                CancellationToken))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("VerifyEmail.Failed", result.Error.Code);
        Assert.Contains("Unexpected error", result.Error.Message);
    }
}