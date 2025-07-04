using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Features.Identity.Commands.ChangePassword.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Application.UnitTests.Testing;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.Errors;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Identity.Commands;

public class ChangePasswordCommandTests : TestBase
{
    private readonly IMediator _mediator;

    public ChangePasswordCommandTests()
    {
        var services = new ServiceCollection();
        services.AddTransient(sp => Fixture.MockAuthenticationService.Object);
        services.AddTransient(sp => Fixture.GetLogger<ChangePasswordCommandHandlerV1>());

        services.AddMediatRWithValidation();

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task ChangePassword_WithValidData_ReturnsSuccessfulResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommandV1
        {
            UserId = userId,
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword456!",
            ConfirmPassword = "NewPassword456!"
        };

        // Mock successful password change
        Fixture.MockAuthenticationService
            .Setup(auth => auth.ChangePasswordAsync(
                userId,
                command.CurrentPassword,
                command.NewPassword,
                CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify auth service was called with correct parameters
        Fixture.MockAuthenticationService.Verify(auth => auth.ChangePasswordAsync(
            userId,
            command.CurrentPassword,
            command.NewPassword,
            CancellationToken), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_WithPasswordMismatch_ReturnsErrorResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommandV1
        {
            UserId = userId,
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword456!",
            ConfirmPassword = "DifferentPassword789!" // Different from NewPassword
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User.PasswordMismatch", result.Error.Code);

        // Verify auth service was not called
        Fixture.MockAuthenticationService.Verify(auth => auth.ChangePasswordAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_WithInvalidCurrentPassword_ReturnsErrorResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommandV1
        {
            UserId = userId,
            CurrentPassword = "WrongPassword123!",
            NewPassword = "NewPassword456!",
            ConfirmPassword = "NewPassword456!"
        };

        // Mock auth service to return invalid credentials error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.ChangePasswordAsync(
                userId,
                command.CurrentPassword,
                command.NewPassword,
                CancellationToken))
            .ReturnsAsync(Result.Failure(UserErrors.InvalidCredentials));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.InvalidCredentials.Code, result.Error.Code);
    }

    [Fact]
    public async Task ChangePassword_WithSameNewAndCurrentPassword_ReturnsValidationError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var password = "Password123!";
        var command = new ChangePasswordCommandV1
        {
            UserId = userId,
            CurrentPassword = password,
            NewPassword = password, // Same as current
            ConfirmPassword = password
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation.Failed", result.Error.Code);

        // Verify auth service was not called
        Fixture.MockAuthenticationService.Verify(auth => auth.ChangePasswordAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_WithWeakNewPassword_ReturnsValidationError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommandV1
        {
            UserId = userId,
            CurrentPassword = "OldPassword123!",
            NewPassword = "weak", // Too weak
            ConfirmPassword = "weak"
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.PasswordTooShort.Code, result.Error.Code);

        // Check for multiple password validation errors
        // Assert.NotNull(result.Error.Metadata);
        // Assert.Contains("NewPassword", result.Error.Metadata.Keys);

        // Verify auth service was not called
        Fixture.MockAuthenticationService.Verify(auth => auth.ChangePasswordAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_WithEmptyUserId_ReturnsValidationError()
    {
        // Arrange
        var command = new ChangePasswordCommandV1
        {
            UserId = Guid.Empty, // Empty UserId
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword456!",
            ConfirmPassword = "NewPassword456!"
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation.Failed", result.Error.Code);

        // Verify metadata contains user id validation error
        Assert.NotNull(result.Error.Metadata);
        Assert.Contains("UserId", result.Error.Metadata.Keys);

        // Verify auth service was not called
        Fixture.MockAuthenticationService.Verify(auth => auth.ChangePasswordAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_WithEmptyCurrentPassword_ReturnsValidationError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommandV1
        {
            UserId = userId,
            CurrentPassword = "", // Empty current password
            NewPassword = "NewPassword456!",
            ConfirmPassword = "NewPassword456!"
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.PasswordRequired.Code, result.Error.Code);
    }

    [Fact]
    public async Task ChangePassword_WhenExceptionOccurs_ReturnsFailureResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommandV1
        {
            UserId = userId,
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword456!",
            ConfirmPassword = "NewPassword456!"
        };

        // Mock auth service to throw an exception
        Fixture.MockAuthenticationService
            .Setup(auth => auth.ChangePasswordAsync(
                userId,
                command.CurrentPassword,
                command.NewPassword,
                CancellationToken))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("ChangePassword.Failed", result.Error.Code);
        Assert.Contains("Unexpected error", result.Error.Message);
    }

    [Fact]
    public async Task ChangePassword_WithUserNotFound_ReturnsErrorResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommandV1
        {
            UserId = userId,
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword456!",
            ConfirmPassword = "NewPassword456!"
        };

        // Mock auth service to return user not found error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.ChangePasswordAsync(
                userId,
                command.CurrentPassword,
                command.NewPassword,
                CancellationToken))
            .ReturnsAsync(Result.Failure(UserErrors.NotFound(userId)));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound(userId).Code, result.Error.Code);
    }
}