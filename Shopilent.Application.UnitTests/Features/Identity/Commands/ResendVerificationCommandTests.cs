using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Features.Identity.Commands.ResendVerification.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Application.UnitTests.Testing;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.Errors;
using Shopilent.Domain.Identity.ValueObjects;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Identity.Commands;

public class ResendVerificationCommandTests : TestBase
{
    private readonly IMediator _mediator;

    public ResendVerificationCommandTests()
    {
        var services = new ServiceCollection();
        services.AddTransient(sp => Fixture.MockAuthenticationService.Object);
        services.AddTransient(sp => Fixture.GetLogger<ResendVerificationCommandHandlerV1>());

        services.AddMediatRWithValidation();

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task ResendVerification_WithValidEmail_ReturnsSuccessfulResult()
    {
        // Arrange
        var command = new ResendVerificationCommandV1
        {
            Email = "test@example.com"
        };

        // Mock successful email verification
        Fixture.MockAuthenticationService
            .Setup(auth => auth.SendEmailVerificationAsync(
                It.IsAny<Email>(),
                CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify auth service was called with correct email
        Fixture.MockAuthenticationService.Verify(
            auth => auth.SendEmailVerificationAsync(
                It.Is<Email>(e => e.Value == command.Email),
                CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ResendVerification_WithInvalidEmailFormat_ReturnsValidationError()
    {
        // Arrange
        var command = new ResendVerificationCommandV1
        {
            Email = "invalid-email"
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User.InvalidEmailFormat", result.Error.Code);

        // Verify auth service was not called
        Fixture.MockAuthenticationService.Verify(
            auth => auth.SendEmailVerificationAsync(
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResendVerification_WithEmptyEmail_ReturnsValidationError()
    {
        // Arrange
        var command = new ResendVerificationCommandV1
        {
            Email = ""
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User.EmailRequired", result.Error.Code);

        // Verify auth service was not called
        Fixture.MockAuthenticationService.Verify(
            auth => auth.SendEmailVerificationAsync(
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResendVerification_WithNonExistentEmail_ReturnsErrorResult()
    {
        // Arrange
        var command = new ResendVerificationCommandV1
        {
            Email = "nonexistent@example.com"
        };

        // Mock user not found error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.SendEmailVerificationAsync(
                It.IsAny<Email>(),
                CancellationToken))
            .ReturnsAsync(Result.Failure(UserErrors.NotFound(Guid.Empty)));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound(Guid.Empty).Code, result.Error.Code);
    }

    [Fact]
    public async Task ResendVerification_ForAlreadyVerifiedEmail_ReturnsSuccessResult()
    {
        // Arrange
        var command = new ResendVerificationCommandV1
        {
            Email = "verified@example.com"
        };

        // Mock already verified email (the service should handle this gracefully)
        Fixture.MockAuthenticationService
            .Setup(auth => auth.SendEmailVerificationAsync(
                It.IsAny<Email>(),
                CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ResendVerification_WhenEmailServiceFails_ReturnsErrorResult()
    {
        // Arrange
        var command = new ResendVerificationCommandV1
        {
            Email = "test@example.com"
        };

        // Mock email service failure
        Fixture.MockAuthenticationService
            .Setup(auth => auth.SendEmailVerificationAsync(
                It.IsAny<Email>(),
                CancellationToken))
            .ReturnsAsync(Result.Failure(Error.Failure(
                code: "Email.SendingFailed",
                message: "Failed to send verification email")));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Email.SendingFailed", result.Error.Code);
    }

    [Fact]
    public async Task ResendVerification_WhenExceptionOccurs_ReturnsFailureResult()
    {
        // Arrange
        var command = new ResendVerificationCommandV1
        {
            Email = "test@example.com"
        };

        // Mock auth service to throw an exception
        Fixture.MockAuthenticationService
            .Setup(auth => auth.SendEmailVerificationAsync(
                It.IsAny<Email>(),
                CancellationToken))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("ResendVerification.Failed", result.Error.Code);
        Assert.Contains("Unexpected error", result.Error.Message);
    }
}