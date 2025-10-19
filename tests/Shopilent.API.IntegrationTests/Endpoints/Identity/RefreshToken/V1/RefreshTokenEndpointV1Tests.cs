using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.RefreshToken.V1;

public class RefreshTokenEndpointV1Tests : ApiIntegrationTestBase
{
    public RefreshTokenEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange - First login to get a valid refresh token
        await EnsureAdminUserExistsAsync();
        var loginResponse = await PostApiResponseAsync<object, LoginResponse>("v1/auth/login",
            new { Email = "admin@shopilent.com", Password = "Admin123!" });

        AssertApiSuccess(loginResponse);
        var validRefreshToken = loginResponse!.Data.RefreshToken;
        var request = RefreshTokenTestDataV1.CreateValidRequest(validRefreshToken);

        // Act
        var response = await PostApiResponseAsync<object, RefreshTokenResponseV1>("v1/auth/refresh-token", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().NotBeEmpty();
        response.Data.Email.Should().Be("admin@shopilent.com");
        response.Data.FullName.Should().NotBeNullOrEmpty();
        response.Data.AccessToken.Should().NotBeNullOrEmpty();
        response.Data.RefreshToken.Should().NotBeNullOrEmpty();
        response.Data.RefreshToken.Should().NotBe(validRefreshToken); // Should be a new refresh token
        response.Message.Should().Be("Token refresh successful");
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldInvalidateOldToken()
    {
        // Arrange - Login to get a valid refresh token
        await EnsureAdminUserExistsAsync();
        var loginResponse = await PostApiResponseAsync<object, LoginResponse>("v1/auth/login",
            new { Email = "admin@shopilent.com", Password = "Admin123!" });

        AssertApiSuccess(loginResponse);
        var validRefreshToken = loginResponse!.Data.RefreshToken;
        var request = RefreshTokenTestDataV1.CreateValidRequest(validRefreshToken);

        // Act - Use the refresh token first time
        var firstRefreshResponse =
            await PostApiResponseAsync<object, RefreshTokenResponseV1>("v1/auth/refresh-token", request);
        AssertApiSuccess(firstRefreshResponse);

        // Act - Try to use the same refresh token again
        var secondRefreshResponse = await PostAsync("v1/auth/refresh-token", request);

        // Assert - Second attempt should fail
        secondRefreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await secondRefreshResponse.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("invalid", "expired", "revoked", "not found");
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = RefreshTokenTestDataV1.CreateRequestWithExpiredRefreshToken();

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("invalid", "expired", "unauthorized", "not found");
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = RefreshTokenTestDataV1.CreateRequestWithInvalidRefreshToken();

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("invalid", "unauthorized", "not found");
    }

    [Fact]
    public async Task RefreshToken_WithEmptyToken_ShouldReturnValidationError()
    {
        // Arrange
        var request = RefreshTokenTestDataV1.CreateRequestWithEmptyRefreshToken();

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("required", "empty", "RefreshToken");
    }

    [Fact]
    public async Task RefreshToken_WithNullToken_ShouldReturnValidationError()
    {
        // Arrange
        var request = RefreshTokenTestDataV1.CreateRequestWithNullRefreshToken();

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("required", "RefreshToken");
    }

    [Fact]
    public async Task RefreshToken_WithWhitespaceToken_ShouldReturnValidationError()
    {
        // Arrange
        var request = RefreshTokenTestDataV1.CreateRequestWithWhitespaceRefreshToken();

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("required", "empty", "RefreshToken");
    }

    [Fact]
    public async Task RefreshToken_WithRevokedToken_ShouldReturnUnauthorized()
    {
        // Arrange - First login to get a valid refresh token
        await EnsureAdminUserExistsAsync();
        var loginResponse = await PostApiResponseAsync<object, LoginResponse>("v1/auth/login",
            new { Email = "admin@shopilent.com", Password = "Admin123!" });

        AssertApiSuccess(loginResponse);
        var validRefreshToken = loginResponse!.Data.RefreshToken;

        // Revoke the token by logging out
        SetAuthenticationHeader(loginResponse.Data.AccessToken);
        var logoutResponse = await PostAsync("v1/auth/logout", new { RefreshToken = validRefreshToken });
        logoutResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        // Clear auth header for the refresh attempt
        ClearAuthenticationHeader();
        var request = RefreshTokenTestDataV1.CreateValidRequest(validRefreshToken);

        // Act - Try to use the revoked refresh token
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("invalid", "revoked", "unauthorized", "not found");
    }

    [Fact]
    public async Task RefreshToken_WithCustomerToken_ShouldReturnSuccess()
    {
        // Arrange - Login as customer to get a valid refresh token
        await EnsureCustomerUserExistsAsync();
        var loginResponse = await PostApiResponseAsync<object, LoginResponse>("v1/auth/login",
            new { Email = "customer@shopilent.com", Password = "Customer123!" });

        AssertApiSuccess(loginResponse);
        var validRefreshToken = loginResponse!.Data.RefreshToken;
        var request = RefreshTokenTestDataV1.CreateValidRequest(validRefreshToken);

        // Act
        var response = await PostApiResponseAsync<object, RefreshTokenResponseV1>("v1/auth/refresh-token", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Email.Should().Be("customer@shopilent.com");
        response.Data.AccessToken.Should().NotBeNullOrEmpty();
        response.Data.RefreshToken.Should().NotBeNullOrEmpty();
        response.Data.RefreshToken.Should().NotBe(validRefreshToken);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-token")]
    [InlineData("definitely.not.a.valid.refresh.token")]
    public async Task RefreshToken_WithInvalidTokenFormats_ShouldReturnUnauthorizedOrBadRequest(string? invalidToken)
    {
        // Arrange
        var request = RefreshTokenTestDataV1.CreateValidRequest(invalidToken);

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    // Security tests
    [Fact]
    public async Task RefreshToken_WithSqlInjectionAttempt_ShouldReturnUnauthorized()
    {
        // Arrange - First ensure we have a user in the database
        await EnsureAdminUserExistsAsync();

        // Get initial user count to verify database integrity later
        var initialUserCount = await ExecuteDbContextAsync(async context =>
        {
            return await context.Users.CountAsync();
        });

        var request = RefreshTokenTestDataV1.SecurityTests.CreateSqlInjectionAttempt();

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Verify database integrity - user count should remain the same
        await ExecuteDbContextAsync(async context =>
        {
            var finalUserCount = await context.Users.CountAsync();
            finalUserCount.Should().Be(initialUserCount); // Database should not be affected
            finalUserCount.Should().BeGreaterThan(0); // Should still have users
        });
    }

    [Fact]
    public async Task RefreshToken_WithXssAttempt_ShouldRejectSecurely()
    {
        // Arrange
        var request = RefreshTokenTestDataV1.SecurityTests.CreateXssAttempt();

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert - Should reject with either BadRequest or Unauthorized
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("<script>"); // Should not echo back the XSS payload
    }

    [Fact]
    public async Task RefreshToken_WithLongTokenAttack_ShouldRejectSecurely()
    {
        // Arrange
        var request = RefreshTokenTestDataV1.SecurityTests.CreateLongTokenAttack();

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    // Edge case tests
    [Fact]
    public async Task RefreshToken_WithUnicodeCharacters_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = RefreshTokenTestDataV1.EdgeCases.CreateRequestWithUnicodeCharacters();

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithBase64Token_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = RefreshTokenTestDataV1.EdgeCases.CreateRequestWithBase64Token();

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithGuidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = RefreshTokenTestDataV1.EdgeCases.CreateRequestWithGuidToken();

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Boundary value tests
    [Fact]
    public async Task RefreshToken_WithMinimumLengthToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = RefreshTokenTestDataV1.BoundaryTests.CreateRequestWithMinimumLengthToken();

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithMaximumLengthToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = RefreshTokenTestDataV1.BoundaryTests.CreateRequestWithMaximumLengthToken();

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithTooLongToken_ShouldReturnBadRequest()
    {
        // Arrange
        var request = RefreshTokenTestDataV1.BoundaryTests.CreateRequestWithTooLongToken();

        // Act
        var response = await PostAsync("v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("too long", "maximum", "255");
    }

    // Concurrent request tests
    [Fact]
    public async Task RefreshToken_ConcurrentRequestsWithSameToken_ShouldHandleGracefully()
    {
        // Arrange - Login to get a valid refresh token
        await EnsureAdminUserExistsAsync();
        var loginResponse = await PostApiResponseAsync<object, LoginResponse>("v1/auth/login",
            new { Email = "admin@shopilent.com", Password = "Admin123!" });

        AssertApiSuccess(loginResponse);
        var validRefreshToken = loginResponse!.Data.RefreshToken;
        var request = RefreshTokenTestDataV1.CreateValidRequest(validRefreshToken);

        // Act - Make multiple concurrent requests with the same token
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => PostAsync("v1/auth/refresh-token", request))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert - System should handle concurrent requests without crashing
        // All responses should be either OK or Unauthorized (no server errors)
        responses.Should().AllSatisfy(response =>
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        });

        // At least one request should succeed
        var successfulResponses = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        successfulResponses.Should().BeGreaterThan(0);

        // If there are successful responses, they should all return valid access tokens
        foreach (var response in responses.Where(r => r.StatusCode == HttpStatusCode.OK))
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("accessToken");
        }
    }

    // Response DTO for this specific endpoint version
    public class RefreshTokenResponseV1
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
