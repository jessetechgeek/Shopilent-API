using Microsoft.EntityFrameworkCore;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Domain.Identity.Enums;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Infrastructure.IntegrationTests.Common;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Identity.Services.Authentication;

[Collection("IntegrationTests")]
public class AuthenticationServiceTests : IntegrationTestBase
{
    private IAuthenticationService _authenticationService = null!;

    public AuthenticationServiceTests(IntegrationTestFixture integrationTestFixture)
        : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _authenticationService = GetService<IAuthenticationService>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldReturnJwtTokens()
    {
        // Arrange
        await ResetDatabaseAsync();

        var password = "TestPassword123!";
        var email = Email.Create("testuser@example.com").Value;

        // Register user first
        var registerResult = await _authenticationService.RegisterAsync(
            email,
            password,
            "John",
            "Doe",
            "+1234567890",
            "192.168.1.1",
            "Test Browser");

        registerResult.IsSuccess.Should().BeTrue();

        // Act
        var loginResult = await _authenticationService.LoginAsync(
            email,
            password,
            "192.168.1.1",
            "Test Browser");

        // Assert
        loginResult.IsSuccess.Should().BeTrue();
        loginResult.Value.Should().NotBeNull();
        loginResult.Value.AccessToken.Should().NotBeNullOrEmpty();
        loginResult.Value.RefreshToken.Should().NotBeNullOrEmpty();
        loginResult.Value.User.Should().NotBeNull();
        loginResult.Value.User.Email.Value.Should().Be(email.Value);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        await ResetDatabaseAsync();

        var password = "TestPassword123!";
        var email = Email.Create("testuser@example.com").Value;

        // Register user first
        await _authenticationService.RegisterAsync(
            email,
            password,
            "John",
            "Doe");

        var wrongPassword = "WrongPassword123!";

        // Act
        var result = await _authenticationService.LoginAsync(email, wrongPassword);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidCredentials");
    }

    [Fact]
    public async Task Register_ValidUser_ShouldCreateAccountAndTokens()
    {
        // Arrange
        await ResetDatabaseAsync();

        var email = Email.Create("newuser@example.com").Value;
        var password = "NewPassword123!";

        // Act
        var result = await _authenticationService.RegisterAsync(
            email,
            password,
            "John",
            "Doe",
            "+1234567890");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        result.Value.User.Should().NotBeNull();
        result.Value.User.Email.Value.Should().Be(email.Value);
        result.Value.User.FullName.FirstName.Should().Be("John");
        result.Value.User.FullName.LastName.Should().Be("Doe");
        result.Value.User.Role.Should().Be(UserRole.Customer);

        // Verify user was created in database
        var userFromDb = await DbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email.Value == email.Value);

        userFromDb.Should().NotBeNull();
        userFromDb!.RefreshTokens.Should().HaveCount(1);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        await ResetDatabaseAsync();

        var email = Email.Create("existing@example.com").Value;
        var password = "TestPassword123!";

        // Register first user
        await _authenticationService.RegisterAsync(
            email,
            password,
            "John",
            "Doe");

        // Act - try to register with same email
        var result = await _authenticationService.RegisterAsync(
            email,
            password,
            "Jane",
            "Smith");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmailAlreadyExists");
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        await ResetDatabaseAsync();

        var email = Email.Create("testuser@example.com").Value;
        var password = "TestPassword123!";

        // Register and login to get initial tokens
        var registerResult = await _authenticationService.RegisterAsync(
            email,
            password,
            "John",
            "Doe");

        registerResult.IsSuccess.Should().BeTrue();
        var initialRefreshToken = registerResult.Value.RefreshToken;

        // Act
        var result = await _authenticationService.RefreshTokenAsync(initialRefreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBe(initialRefreshToken); // Should be a new token
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_ShouldReturnFailure()
    {
        // Arrange
        await ResetDatabaseAsync();

        var invalidToken = "invalid_refresh_token_123";

        // Act
        var result = await _authenticationService.RefreshTokenAsync(invalidToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RefreshToken.NotFound");
    }

    [Fact]
    public async Task RevokeToken_ValidToken_ShouldInvalidateRefreshToken()
    {
        // Arrange
        await ResetDatabaseAsync();

        var email = Email.Create("testuser@example.com").Value;
        var password = "TestPassword123!";

        // Register to get tokens
        var registerResult = await _authenticationService.RegisterAsync(
            email,
            password,
            "John",
            "Doe");

        registerResult.IsSuccess.Should().BeTrue();
        var refreshToken = registerResult.Value.RefreshToken;

        // Act
        var result = await _authenticationService.RevokeTokenAsync(refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify token is revoked by trying to use it
        var refreshResult = await _authenticationService.RefreshTokenAsync(refreshToken);
        refreshResult.IsFailure.Should().BeTrue();
    }
}
