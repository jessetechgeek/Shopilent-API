using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Features.Identity.Commands.ForgotPassword.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Application.UnitTests.Testing;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.Errors;
using Shopilent.Domain.Identity.ValueObjects;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Identity.Commands;

public class ForgotPasswordCommandTests : TestBase
{
    private readonly IMediator _mediator;

    public ForgotPasswordCommandTests()
    {
        var services = new ServiceCollection();
        services.AddTransient(sp => Fixture.MockAuthenticationService.Object);
        services.AddTransient(sp => Fixture.GetLogger<ForgotPasswordCommandHandlerV1>());

        services.AddMediatRWithValidation();

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ReturnsSuccessfulResult()
    {
        // Arrange
        var command = new ForgotPasswordCommandV1
        {
            Email = "test@example.com"
        };

        // Mock successful password reset request
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RequestPasswordResetAsync(
                It.IsAny<Email>(),
                CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify auth service was called with correct email
        Fixture.MockAuthenticationService.Verify(auth => auth.RequestPasswordResetAsync(
            It.Is<Email>(e => e.Value == command.Email),
            CancellationToken), Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_WithInvalidEmail_ReturnsErrorResult()
    {
        // Arrange
        var command = new ForgotPasswordCommandV1
        {
            Email = "invalid-email"
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User.InvalidEmailFormat", result.Error.Code);

        // Verify auth service was not called
        Fixture.MockAuthenticationService.Verify(auth => auth.RequestPasswordResetAsync(
            It.IsAny<Email>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_WithEmptyEmail_ReturnsValidationError()
    {
        // Arrange
        var command = new ForgotPasswordCommandV1
        {
            Email = ""
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.EmailRequired.Code, result.Error.Code);

        // Verify auth service was not called
        Fixture.MockAuthenticationService.Verify(auth => auth.RequestPasswordResetAsync(
            It.IsAny<Email>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_WithNonExistentEmail_ReturnsSuccessAnyway()
    {
        // Arrange
        var command = new ForgotPasswordCommandV1
        {
            Email = "nonexistent@example.com"
        };

        // Mock auth service to return user not found, but this should not
        // be exposed to the client for security reasons
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RequestPasswordResetAsync(
                It.IsAny<Email>(),
                CancellationToken))
            .ReturnsAsync(Result.Success()); // Still returns success to prevent user enumeration

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Even though user doesn't exist, we should still call the service
        // and return success to prevent user enumeration attacks
        Fixture.MockAuthenticationService.Verify(auth => auth.RequestPasswordResetAsync(
            It.Is<Email>(e => e.Value == command.Email),
            CancellationToken), Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailSendingFails_ReturnsErrorResult()
    {
        // Arrange
        var command = new ForgotPasswordCommandV1
        {
            Email = "test@example.com"
        };

        // Mock auth service to return email sending error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RequestPasswordResetAsync(
                It.IsAny<Email>(),
                CancellationToken))
            .ReturnsAsync(Result.Failure(Error.Failure(
                code: "Email.SendingFailed",
                message: "Failed to send email")));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Email.SendingFailed", result.Error.Code);
    }

    [Fact]
    public async Task ForgotPassword_WhenExceptionOccurs_ReturnsFailureResult()
    {
        // Arrange
        var command = new ForgotPasswordCommandV1
        {
            Email = "test@example.com"
        };

        // Mock auth service to throw an exception
        Fixture.MockAuthenticationService
            .Setup(auth => auth.RequestPasswordResetAsync(
                It.IsAny<Email>(),
                CancellationToken))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("ForgotPassword.Failed", result.Error.Code);
        Assert.Contains("Unexpected error", result.Error.Message);
    }
}