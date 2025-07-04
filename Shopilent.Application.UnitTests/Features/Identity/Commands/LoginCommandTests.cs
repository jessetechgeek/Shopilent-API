using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Features.Identity.Commands.Login.V1;
using Shopilent.Application.UnitTests.Common;
using Shopilent.Application.UnitTests.Testing;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.Errors;
using Shopilent.Domain.Identity.ValueObjects;
using Xunit;

namespace Shopilent.Application.UnitTests.Features.Identity.Commands;

public class LoginCommandTests : TestBase
{
    private readonly IMediator _mediator;

    public LoginCommandTests()
    {
        var services = new ServiceCollection();
        services.AddTransient(sp => Fixture.MockAuthenticationService.Object);
        services.AddTransient(sp => Fixture.GetLogger<LoginCommandHandlerV1>());

        services.AddMediatRWithValidation();

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccessfulResult()
    {
        // Arrange
        var command = new LoginCommandV1
        {
            Email = "test@example.com",
            Password = "Password123!",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        var expectedResponse = new AuthTokenResponse
        {
            User = null,
            AccessToken = "test-access-token",
            RefreshToken = "test-refresh-token"
        };

        // Mock successful login
        Fixture.MockAuthenticationService
            .Setup(auth => auth.LoginAsync(
                It.IsAny<Email>(),
                command.Password,
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

        // Verify that the Authentication service was called with the correct parameters
        Fixture.MockAuthenticationService.Verify(
            auth => auth.LoginAsync(
                It.Is<Email>(e => e.Value == command.Email),
                command.Password,
                command.IpAddress,
                command.UserAgent,
                CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsErrorResult()
    {
        // Arrange
        var command = new LoginCommandV1
        {
            Email = "invalid-email",
            Password = "Password123!",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation.Failed", result.Error.Code);

        // Verify metadata contains email validation error
        Assert.NotNull(result.Error.Metadata);
        Assert.Contains("Email", result.Error.Metadata.Keys);

        // Verify that the Authentication service was not called
        Fixture.MockAuthenticationService.Verify(
            auth => auth.LoginAsync(
                It.IsAny<Email>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsErrorResult()
    {
        // Arrange
        var command = new LoginCommandV1
        {
            Email = "test@example.com",
            Password = "WrongPassword",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        // Mock authentication service to return invalid credentials error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.LoginAsync(
                It.IsAny<Email>(),
                command.Password,
                command.IpAddress,
                command.UserAgent,
                CancellationToken))
            .ReturnsAsync(Result.Failure<AuthTokenResponse>(UserErrors.InvalidCredentials));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.InvalidCredentials.Code, result.Error.Code);
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ReturnsValidationError()
    {
        // Arrange
        var command = new LoginCommandV1
        {
            Email = "",
            Password = "Password123!",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User.EmailRequired", result.Error.Code);
        Assert.Contains("Email cannot be empty.", result.Error.Message);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsValidationError()
    {
        // Arrange
        var command = new LoginCommandV1
        {
            Email = "test@example.com",
            Password = "",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User.PasswordRequired", result.Error.Code);
        Assert.Contains("Password hash cannot be empty.", result.Error.Message);
    }

    [Fact]
    public async Task Login_WithInactiveAccount_ReturnsErrorResult()
    {
        // Arrange
        var command = new LoginCommandV1
        {
            Email = "inactive@example.com",
            Password = "Password123!",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        // Mock authentication service to return inactive account error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.LoginAsync(
                It.IsAny<Email>(),
                command.Password,
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
    public async Task Login_WithUnverifiedEmail_ReturnsErrorResult()
    {
        // Arrange
        var command = new LoginCommandV1
        {
            Email = "unverified@example.com",
            Password = "Password123!",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        // Mock authentication service to return unverified email error
        Fixture.MockAuthenticationService
            .Setup(auth => auth.LoginAsync(
                It.IsAny<Email>(),
                command.Password,
                command.IpAddress,
                command.UserAgent,
                CancellationToken))
            .ReturnsAsync(Result.Failure<AuthTokenResponse>(UserErrors.EmailNotVerified));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.EmailNotVerified.Code, result.Error.Code);
    }

    [Fact]
    public async Task Login_WhenExceptionOccurs_ReturnsFailureResult()
    {
        // Arrange
        var command = new LoginCommandV1
        {
            Email = "test@example.com",
            Password = "Password123!",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        // Mock authentication service to throw an exception
        Fixture.MockAuthenticationService
            .Setup(auth => auth.LoginAsync(
                It.IsAny<Email>(),
                command.Password,
                command.IpAddress,
                command.UserAgent,
                CancellationToken))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Login.Failed", result.Error.Code);
    }

    [Fact]
    public async Task Login_WithSuccessfulAuthentication_VerifiesUserData()
    {
        // Arrange
        var command = new LoginCommandV1
        {
            Email = "test@example.com",
            Password = "Password123!",
            IpAddress = "127.0.0.1",
            UserAgent = "Test User Agent"
        };

        // Create a proper user object
        var email = Email.Create(command.Email).Value;
        var fullName = FullName.Create("John", "Doe").Value;
        var user = User.Create(email, "hashedPassword", fullName).Value;

        var expectedResponse = new AuthTokenResponse
        {
            User = user,
            AccessToken = "test-access-token",
            RefreshToken = "test-refresh-token"
        };

        // Mock successful login with user data
        Fixture.MockAuthenticationService
            .Setup(auth => auth.LoginAsync(
                It.IsAny<Email>(),
                command.Password,
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
        Assert.NotNull(result.Value.User);
        Assert.Same(user, result.Value.User);
        Assert.Equal(command.Email, result.Value.User.Email.Value);
        Assert.Equal("John", result.Value.User.FullName.FirstName);
        Assert.Equal("Doe", result.Value.User.FullName.LastName);
    }

    [Theory]
    [InlineData(null, "Test User Agent")]
    [InlineData("127.0.0.1", null)]
    [InlineData(null, null)]
    public async Task Login_WithMissingIpOrUserAgent_StillPassesParameters(string ipAddress, string userAgent)
    {
        // Arrange
        var command = new LoginCommandV1
        {
            Email = "test@example.com",
            Password = "Password123!",
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        var expectedResponse = new AuthTokenResponse
        {
            User = null,
            AccessToken = "test-access-token",
            RefreshToken = "test-refresh-token"
        };

        // Mock successful login
        Fixture.MockAuthenticationService
            .Setup(auth => auth.LoginAsync(
                It.IsAny<Email>(),
                command.Password,
                command.IpAddress,
                command.UserAgent,
                CancellationToken))
            .ReturnsAsync(Result.Success(expectedResponse));

        // Act
        var result = await _mediator.Send(command, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify the auth service was called with exactly the provided parameters
        Fixture.MockAuthenticationService.Verify(
            auth => auth.LoginAsync(
                It.IsAny<Email>(),
                command.Password,
                ipAddress,
                userAgent,
                CancellationToken),
            Times.Once);
    }
}