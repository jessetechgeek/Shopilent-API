using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.Logout.V1;

public class LogoutEndpointV1Tests : ApiIntegrationTestBase
{
    public LogoutEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Logout_WithValidRefreshToken_ShouldReturnSuccess()
    {
        // Arrange - First login to get a valid refresh token
        await EnsureAdminUserExistsAsync();
        var loginRequest = new { Email = "admin@shopilent.com", Password = "Admin123!", RememberMe = false };
        var loginResponse = await PostApiResponseAsync<object, LoginResponseDto>("v1/auth/login", loginRequest);
        AssertApiSuccess(loginResponse);

        var refreshToken = loginResponse!.Data.RefreshToken;
        var logoutRequest = LogoutTestDataV1.CreateValidRequest(refreshToken: refreshToken);

        // Set authentication header for the logout request
        SetAuthenticationHeader(loginResponse.Data.AccessToken);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/logout", logoutRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Logout successful");
        response.Message.Should().Be("Logout successful");
    }

    [Fact]
    public async Task Logout_WithValidRefreshToken_ShouldInvalidateTokenInDatabase()
    {
        // Arrange - First login to get a valid refresh token
        await EnsureAdminUserExistsAsync();
        var loginRequest = new { Email = "admin@shopilent.com", Password = "Admin123!", RememberMe = false };
        var loginResponse = await PostApiResponseAsync<object, LoginResponseDto>("v1/auth/login", loginRequest);
        AssertApiSuccess(loginResponse);

        var refreshToken = loginResponse!.Data.RefreshToken;
        var logoutRequest = LogoutTestDataV1.CreateValidRequest(refreshToken: refreshToken);

        // Set authentication header for the logout request
        SetAuthenticationHeader(loginResponse.Data.AccessToken);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/logout", logoutRequest);

        // Assert
        AssertApiSuccess(response);

        // Verify refresh token is invalidated in database
        await ExecuteDbContextAsync(async context =>
        {
            var refreshTokenEntity = await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            // Token should either be deleted or marked as revoked
            if (refreshTokenEntity != null)
            {
                refreshTokenEntity.IsRevoked.Should().BeTrue();
            }
        });
    }

    [Fact]
    public async Task Logout_WithCustomReason_ShouldReturnSuccess()
    {
        // Arrange - First login to get a valid refresh token
        await EnsureAdminUserExistsAsync();
        var loginRequest = new { Email = "admin@shopilent.com", Password = "Admin123!", RememberMe = false };
        var loginResponse = await PostApiResponseAsync<object, LoginResponseDto>("v1/auth/login", loginRequest);
        AssertApiSuccess(loginResponse);

        var refreshToken = loginResponse!.Data.RefreshToken;
        var logoutRequest = LogoutTestDataV1.LogoutReasons.CreateRequestWithUserInitiatedLogout();
        
        // Update the request with the actual refresh token
        logoutRequest = new { RefreshToken = refreshToken, Reason = "User initiated logout" };

        // Set authentication header for the logout request
        SetAuthenticationHeader(loginResponse.Data.AccessToken);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/logout", logoutRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Logout successful");
    }

    [Fact]
    public async Task Logout_WithEmptyRefreshToken_ShouldReturnValidationError()
    {
        // Arrange - First ensure admin user exists
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = LogoutTestDataV1.CreateRequestWithEmptyRefreshToken();

        // Act
        var response = await PostAsync("v1/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Refresh token is required.");
    }

    [Fact]
    public async Task Logout_WithNullRefreshToken_ShouldReturnValidationError()
    {
        // Arrange - First ensure admin user exists
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = LogoutTestDataV1.CreateRequestWithNullRefreshToken();

        // Act
        var response = await PostAsync("v1/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Refresh token is required.");
    }

    [Fact]
    public async Task Logout_WithInvalidRefreshToken_ShouldReturnUnauthorized()
    {
        // Arrange - First ensure admin user exists
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = LogoutTestDataV1.CreateRequestWithInvalidRefreshToken();

        // Act
        var response = await PostAsync("v1/auth/logout", request);

        // Assert
        // Invalid refresh tokens that reach the auth service return 401
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Logout_WithNonExistentRefreshToken_ShouldReturnUnauthorized()
    {
        // Arrange - First ensure admin user exists
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        
        // Use a valid format but non-existent refresh token (UUID format)
        var request = new
        {
            RefreshToken = Guid.NewGuid().ToString(),
            Reason = "User logged out"
        };

        // Act
        var response = await PostAsync("v1/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("refresh token was not found");
    }

    [Fact]
    public async Task Logout_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader(); // Ensure no auth header
        var request = LogoutTestDataV1.CreateValidRequest();

        // Act
        var response = await PostAsync("v1/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithExpiredAccessToken_ShouldReturnUnauthorized()
    {
        // Arrange - Use a clearly invalid/expired token
        SetAuthenticationHeader("expired.jwt.token");
        var request = LogoutTestDataV1.CreateValidRequest();

        // Act
        var response = await PostAsync("v1/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Logout_WithInvalidRefreshToken_ShouldReturnValidationError(string? invalidToken)
    {
        // Arrange - First ensure admin user exists
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = new { RefreshToken = invalidToken, Reason = "User logged out" };

        // Act
        var response = await PostAsync("v1/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_WithSameRefreshTokenTwice_ShouldReturnSuccessOnSecondAttempt()
    {
        // Arrange - First login to get a valid refresh token
        await EnsureAdminUserExistsAsync();
        var loginRequest = new { Email = "admin@shopilent.com", Password = "Admin123!", RememberMe = false };
        var loginResponse = await PostApiResponseAsync<object, LoginResponseDto>("v1/auth/login", loginRequest);
        AssertApiSuccess(loginResponse);

        var refreshToken = loginResponse!.Data.RefreshToken;
        var logoutRequest = LogoutTestDataV1.CreateValidRequest(refreshToken: refreshToken);

        // Set authentication header for the logout request
        SetAuthenticationHeader(loginResponse.Data.AccessToken);

        // Act - First logout should succeed
        var firstResponse = await PostApiResponseAsync<object, string>("v1/auth/logout", logoutRequest);
        AssertApiSuccess(firstResponse);

        // Act - Second logout with same refresh token should also succeed (idempotent)
        var secondResponse = await PostApiResponseAsync<object, string>("v1/auth/logout", logoutRequest);

        // Assert - Second logout succeeds because token is already revoked (idempotent operation)
        AssertApiSuccess(secondResponse);
        secondResponse!.Data.Should().Be("Logout successful");
    }

    // Security tests
    [Fact]
    public async Task Logout_WithSqlInjectionAttempt_ShouldReturnBadRequestSafely()
    {
        // Arrange - First ensure admin user exists
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = LogoutTestDataV1.SecurityTests.CreateSqlInjectionAttempt();

        // Act
        var response = await PostAsync("v1/auth/logout", request);

        // Assert - Should handle safely without exposing system information
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("SQL");
        content.Should().NotContain("database");
        content.Should().NotContain("exception");
    }

    [Fact]
    public async Task Logout_WithXssAttempt_ShouldReturnBadRequestSafely()
    {
        // Arrange - First ensure admin user exists
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = LogoutTestDataV1.SecurityTests.CreateXssAttempt();

        // Act
        var response = await PostAsync("v1/auth/logout", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("<script>");
        content.Should().NotContain("alert");
    }

    [Fact]
    public async Task Logout_WithExcessivelyLongRefreshToken_ShouldReturnValidationError()
    {
        // Arrange - First ensure admin user exists
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = LogoutTestDataV1.SecurityTests.CreateLongRefreshTokenAttack();

        // Act
        var response = await PostAsync("v1/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Refresh token is too long");
    }

    [Fact]
    public async Task Logout_WithExcessivelyLongReason_ShouldReturnValidationError()
    {
        // Arrange - First ensure admin user exists
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = LogoutTestDataV1.SecurityTests.CreateLongReasonAttack();

        // Act
        var response = await PostAsync("v1/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Reason is too long");
    }

    // Edge cases
    [Fact]
    public async Task Logout_WithUnicodeReason_ShouldReturnSuccess()
    {
        // Arrange - First login to get a valid refresh token
        await EnsureAdminUserExistsAsync();
        var loginRequest = new { Email = "admin@shopilent.com", Password = "Admin123!", RememberMe = false };
        var loginResponse = await PostApiResponseAsync<object, LoginResponseDto>("v1/auth/login", loginRequest);
        AssertApiSuccess(loginResponse);

        var refreshToken = loginResponse!.Data.RefreshToken;
        var request = new 
        { 
            RefreshToken = refreshToken, 
            Reason = "Üser lögöut with spëcial çharacters" 
        };

        // Set authentication header for the logout request
        SetAuthenticationHeader(loginResponse.Data.AccessToken);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/logout", request);

        // Assert
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task Logout_WithWhitespaceReason_ShouldHandleCorrectly()
    {
        // Arrange - First login to get a valid refresh token
        await EnsureAdminUserExistsAsync();
        var loginRequest = new { Email = "admin@shopilent.com", Password = "Admin123!", RememberMe = false };
        var loginResponse = await PostApiResponseAsync<object, LoginResponseDto>("v1/auth/login", loginRequest);
        AssertApiSuccess(loginResponse);

        var refreshToken = loginResponse!.Data.RefreshToken;
        var request = new 
        { 
            RefreshToken = refreshToken, 
            Reason = "  User logged out  " 
        };

        // Set authentication header for the logout request
        SetAuthenticationHeader(loginResponse.Data.AccessToken);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/logout", request);

        // Assert
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task Logout_WithOnlyWhitespaceReason_ShouldReturnSuccess()
    {
        // Arrange - First login to get a valid refresh token
        await EnsureAdminUserExistsAsync();
        var loginRequest = new { Email = "admin@shopilent.com", Password = "Admin123!", RememberMe = false };
        var loginResponse = await PostApiResponseAsync<object, LoginResponseDto>("v1/auth/login", loginRequest);
        AssertApiSuccess(loginResponse);

        var refreshToken = loginResponse!.Data.RefreshToken;
        var request = new 
        { 
            RefreshToken = refreshToken, 
            Reason = "   " // Only whitespace reason should be accepted since no validation
        };

        // Set authentication header for the logout request
        SetAuthenticationHeader(loginResponse.Data.AccessToken);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/logout", request);

        // Assert - Should succeed since Reason field has no validation rules
        AssertApiSuccess(response);
    }

    // Boundary tests
    [Fact]
    public async Task Logout_WithMinimumValidReason_ShouldReturnSuccess()
    {
        // Arrange - First login to get a valid refresh token
        await EnsureAdminUserExistsAsync();
        var loginRequest = new { Email = "admin@shopilent.com", Password = "Admin123!", RememberMe = false };
        var loginResponse = await PostApiResponseAsync<object, LoginResponseDto>("v1/auth/login", loginRequest);
        AssertApiSuccess(loginResponse);

        var refreshToken = loginResponse!.Data.RefreshToken;
        var request = new 
        { 
            RefreshToken = refreshToken, 
            Reason = "a" // Minimum length reason
        };

        // Set authentication header for the logout request
        SetAuthenticationHeader(loginResponse.Data.AccessToken);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/logout", request);

        // Assert
        AssertApiSuccess(response);
    }

    // Multiple logout scenarios
    [Fact]
    public async Task Logout_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange - Login multiple times to get different refresh tokens
        await EnsureAdminUserExistsAsync();
        var loginRequest = new { Email = "admin@shopilent.com", Password = "Admin123!", RememberMe = false };
        
        var loginTasks = Enumerable.Range(0, 3)
            .Select(_ => PostApiResponseAsync<object, LoginResponseDto>("v1/auth/login", loginRequest))
            .ToList();

        var loginResponses = await Task.WhenAll(loginTasks);
        loginResponses.Should().AllSatisfy(response => AssertApiSuccess(response));

        var logoutTasks = loginResponses
            .Select(loginResponse => new
            {
                RefreshToken = loginResponse!.Data.RefreshToken,
                AccessToken = loginResponse.Data.AccessToken
            })
            .Select(tokens =>
            {
                var request = LogoutTestDataV1.CreateValidRequest(refreshToken: tokens.RefreshToken);
                SetAuthenticationHeader(tokens.AccessToken);
                return PostAsync("v1/auth/logout", request);
            })
            .ToList();

        // Act
        var responses = await Task.WhenAll(logoutTasks);

        // Assert - All should succeed
        responses.Should().AllSatisfy(response =>
            response.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task Logout_AfterUserPasswordChange_ShouldStillWork()
    {
        // Arrange - First login to get a valid refresh token
        await EnsureAdminUserExistsAsync();
        var loginRequest = new { Email = "admin@shopilent.com", Password = "Admin123!", RememberMe = false };
        var loginResponse = await PostApiResponseAsync<object, LoginResponseDto>("v1/auth/login", loginRequest);
        AssertApiSuccess(loginResponse);

        var refreshToken = loginResponse!.Data.RefreshToken;
        var logoutRequest = LogoutTestDataV1.LogoutReasons.CreateRequestWithPasswordChange();
        
        // Update the request with the actual refresh token
        logoutRequest = new { RefreshToken = refreshToken, Reason = "Password changed - logout required" };

        // Set authentication header for the logout request
        SetAuthenticationHeader(loginResponse.Data.AccessToken);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/logout", logoutRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Logout successful");
    }

    [Fact]
    public async Task Logout_WithDifferentReasons_ShouldReturnSuccess()
    {
        // Test various logout reasons
        var reasons = new[]
        {
            "Session timeout",
            "Security logout - suspicious activity detected",
            "Administrative logout",
            "User initiated logout"
        };

        foreach (var reason in reasons)
        {
            // Arrange - Login for each test
            await EnsureAdminUserExistsAsync();
            var loginRequest = new { Email = "admin@shopilent.com", Password = "Admin123!", RememberMe = false };
            var loginResponse = await PostApiResponseAsync<object, LoginResponseDto>("v1/auth/login", loginRequest);
            AssertApiSuccess(loginResponse);

            var refreshToken = loginResponse!.Data.RefreshToken;
            var logoutRequest = new { RefreshToken = refreshToken, Reason = reason };

            // Set authentication header for the logout request
            SetAuthenticationHeader(loginResponse.Data.AccessToken);

            // Act
            var response = await PostApiResponseAsync<object, string>("v1/auth/logout", logoutRequest);

            // Assert
            AssertApiSuccess(response);
            response!.Data.Should().Be("Logout successful");
        }
    }

    // Response DTO for login response (needed for getting refresh tokens)
    public class LoginResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}