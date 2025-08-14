using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Features.Identity.Commands.ResetPassword.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Application.UnitTests.Testing;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.Errors;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Identity.Commands;

public class ResetPasswordCommandTests : TestBase
{
    private readonly IMediator _mediator;

    public ResetPasswordCommandTests()
    {
        var services = new ServiceCollection();
        services.AddTransient(sp => Fixture.MockAuthenticationService.Object);
        services.AddTransient(sp => Fixture.GetLogger<ResetPasswordCommandHandlerV1>());

        services.AddMediatRWithValidation();

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task ResetPassword_WithValidData_ReturnsSuccessfulResult()
    {
        // Arrange
        var command = new ResetPasswordCommandV1
        {
            Token = "valid-reset-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Mock successful password reset
        Fixture.MockAuthenticationService
            .Setup(auth => auth.ResetPasswordAsync(
                command.Token,
                command.NewPassword,
                CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify auth service was called with correct parameters
        Fixture.MockAuthenticationService.Verify(
            auth => auth.ResetPasswordAsync(
                command.Token,
                command.NewPassword,
                CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ResetPassword_WithPasswordMismatch_ReturnsValidationError()
    {
        // Arrange
        var command = new ResetPasswordCommandV1
        {
            Token = "valid-reset-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "DifferentPassword456!" // Different from NewPassword
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("User.PasswordMismatch");

        // Verify auth service was not called
        Fixture.MockAuthenticationService.Verify(
            auth => auth.ResetPasswordAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPassword_WithEmptyToken_ReturnsValidationError()
    {
        // Arrange
        var command = new ResetPasswordCommandV1
        {
            Token = "",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Validation.Failed");

        // Verify auth service was not called
        Fixture.MockAuthenticationService.Verify(
            auth => auth.ResetPasswordAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPassword_WithWeakPassword_ReturnsValidationError()
    {
        // Arrange
        var command = new ResetPasswordCommandV1
        {
            Token = "valid-reset-token",
            NewPassword = "weak", // Too weak
            ConfirmPassword = "weak"
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(UserErrors.PasswordTooShort.Code);

        // Verify auth service was not called
        Fixture.MockAuthenticationService.Verify(
            auth => auth.ResetPasswordAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsErrorResult()
    {
        // Arrange
        var command = new ResetPasswordCommandV1
        {
            Token = "invalid-reset-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Mock invalid token error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.ResetPasswordAsync(
                command.Token,
                command.NewPassword,
                CancellationToken))
            .ReturnsAsync(Result.Failure(Error.Validation(
                code: "PasswordReset.InvalidToken",
                message: "The reset token is invalid or has expired.")));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("PasswordReset.InvalidToken");
    }

    [Fact]
    public async Task ResetPassword_WithExpiredToken_ReturnsErrorResult()
    {
        // Arrange
        var command = new ResetPasswordCommandV1
        {
            Token = "expired-reset-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Mock expired token error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.ResetPasswordAsync(
                command.Token,
                command.NewPassword,
                CancellationToken))
            .ReturnsAsync(Result.Failure(Error.Validation(
                code: "PasswordReset.ExpiredToken",
                message: "The reset token has expired.")));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("PasswordReset.ExpiredToken");
    }

    [Fact]
    public async Task ResetPassword_WhenExceptionOccurs_ReturnsFailureResult()
    {
        // Arrange
        var command = new ResetPasswordCommandV1
        {
            Token = "valid-reset-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Mock auth service to throw an exception
        Fixture.MockAuthenticationService
            .Setup(auth => auth.ResetPasswordAsync(
                command.Token,
                command.NewPassword,
                CancellationToken))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("ResetPassword.Failed");
        result.Error.Message.Should().Contain("Unexpected error");
    }
}