using System.Net;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.VerifyEmail.V1;

public class VerifyEmailEndpointV1Tests : ApiIntegrationTestBase
{
    public VerifyEmailEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task VerifyEmail_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var (userId, verificationToken) = await CreateUnverifiedUserWithTokenAsync();
        
        // Act
        var response = await GetApiResponseAsync<string>($"v1/auth/verify-email/{verificationToken}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Email verification successful. You can now log in.");
        response.Message.Should().Be("Email verification successful");
    }

    [Fact]
    public async Task VerifyEmail_WithValidToken_ShouldMarkEmailAsVerifiedInDatabase()
    {
        // Arrange
        var (userId, verificationToken) = await CreateUnverifiedUserWithTokenAsync();

        // Act
        var response = await GetApiResponseAsync<string>($"v1/auth/verify-email/{verificationToken}");

        // Assert
        AssertApiSuccess(response);

        // Verify user is marked as verified in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            user.Should().NotBeNull();
            user!.EmailVerified.Should().BeTrue();
            user.EmailVerificationToken.Should().BeNull(); // Token should be cleared after use
        });
    }

    [Fact]
    public async Task VerifyEmail_WithEmptyToken_ShouldReturnBadRequestOrNotFound()
    {
        // Arrange
        var emptyToken = VerifyEmailTestDataV1.CreateEmptyToken();

        // Act
        var response = await Client.GetAsync($"v1/auth/verify-email/{emptyToken}");

        // Assert
        // Empty token in path parameter results in route not matching (404) rather than validation error (400)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        
        // 404 responses from routing may have empty content, while 400 validation errors have content
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task VerifyEmail_WithWhitespaceToken_ShouldReturnBadRequestOrNotFound()
    {
        // Arrange
        var whitespaceToken = VerifyEmailTestDataV1.CreateWhitespaceToken();

        // Act
        var response = await Client.GetAsync($"v1/auth/verify-email/{Uri.EscapeDataString(whitespaceToken)}");

        // Assert
        // Whitespace token may be treated as empty by routing, resulting in 404 or validation error (400)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        
        // Only check content for validation errors (400), not routing errors (404)
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task VerifyEmail_WithNonExistentToken_ShouldReturnBadRequest()
    {
        // Arrange
        var nonExistentToken = VerifyEmailTestDataV1.CreateNonExistentToken();

        // Act
        var response = await Client.GetAsync($"v1/auth/verify-email/{nonExistentToken}");

        // Assert
        // Non-existent tokens are treated as validation errors (400) rather than not found (404)
        // This is intentional for security reasons - don't leak information about token existence
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("invalid", "expired", "token");
    }

    [Fact]
    public async Task VerifyEmail_WithExpiredToken_ShouldReturnBadRequest()
    {
        // Arrange
        var (userId, expiredToken) = await CreateUserWithExpiredTokenAsync();

        // Act
        var response = await Client.GetAsync($"v1/auth/verify-email/{expiredToken}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("expired", "invalid", "token");
    }

    [Fact]
    public async Task VerifyEmail_WithAlreadyVerifiedUser_ShouldReturnSuccess()
    {
        // Arrange
        var (userId, token) = await CreateAlreadyVerifiedUserAsync();

        // Act
        var response = await GetApiResponseAsync<string>($"v1/auth/verify-email/{token}");

        // Assert
        // The system allows re-verification as an idempotent operation
        // This prevents errors if users click verification links multiple times
        AssertApiSuccess(response);
        response!.Data.Should().Be("Email verification successful. You can now log in.");
        
        // Verify user remains verified in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            user.Should().NotBeNull();
            user!.EmailVerified.Should().BeTrue();
        });
    }

    [Fact]
    public async Task VerifyEmail_WithUsedToken_ShouldReturnBadRequest()
    {
        // Arrange
        var (userId, verificationToken) = await CreateUnverifiedUserWithTokenAsync();

        // First verification - should succeed
        var firstResponse = await GetApiResponseAsync<string>($"v1/auth/verify-email/{verificationToken}");
        AssertApiSuccess(firstResponse);

        // Act - Second verification with same token should fail
        var response = await Client.GetAsync($"v1/auth/verify-email/{verificationToken}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("invalid", "already", "token");
    }

    [Theory]
    [InlineData("invalid-format")]
    public async Task VerifyEmail_WithInvalidTokenFormats_ShouldReturnBadRequestOrNotFound(string invalidToken)
    {
        // Act
        var response = await Client.GetAsync($"v1/auth/verify-email/{Uri.EscapeDataString(invalidToken)}");

        // Assert
        // Invalid tokens should result in validation errors (400) or not found (404) if token doesn't exist
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        
        // Only check content for validation errors (400), not routing errors (404)
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    // Edge case tests
    [Fact]
    public async Task VerifyEmail_WithExtremelyLongToken_ShouldReturnBadRequestOrNotFound()
    {
        // Arrange
        var extremelyLongToken = VerifyEmailTestDataV1.EdgeCases.CreateExtremelyLongToken();

        // Act
        var response = await Client.GetAsync($"v1/auth/verify-email/{Uri.EscapeDataString(extremelyLongToken)}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task VerifyEmail_WithSpecialCharactersInToken_ShouldHandleGracefully()
    {
        // Arrange
        var specialCharToken = VerifyEmailTestDataV1.EdgeCases.CreateTokenWithSpecialCharacters();

        // Act
        var response = await Client.GetAsync($"v1/auth/verify-email/{Uri.EscapeDataString(specialCharToken)}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task VerifyEmail_WithUnicodeCharactersInToken_ShouldHandleGracefully()
    {
        // Arrange
        var unicodeToken = VerifyEmailTestDataV1.EdgeCases.CreateTokenWithUnicodeCharacters();

        // Act
        var response = await Client.GetAsync($"v1/auth/verify-email/{Uri.EscapeDataString(unicodeToken)}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task VerifyEmail_WithTokenContainingSpaces_ShouldHandleGracefully()
    {
        // Arrange
        var tokenWithSpaces = VerifyEmailTestDataV1.EdgeCases.CreateTokenWithSpaces();

        // Act
        var response = await Client.GetAsync($"v1/auth/verify-email/{Uri.EscapeDataString(tokenWithSpaces)}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task VerifyEmail_WithTokenContainingNewlines_ShouldHandleGracefully()
    {
        // Arrange
        var tokenWithNewlines = VerifyEmailTestDataV1.EdgeCases.CreateTokenWithNewlines();

        // Act
        var response = await Client.GetAsync($"v1/auth/verify-email/{Uri.EscapeDataString(tokenWithNewlines)}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task VerifyEmail_IsAnonymousEndpoint_ShouldNotRequireAuthentication()
    {
        // Arrange
        ClearAuthenticationHeader(); // Ensure no auth header
        var (userId, verificationToken) = await CreateUnverifiedUserWithTokenAsync();

        // Act
        var response = await GetApiResponseAsync<string>($"v1/auth/verify-email/{verificationToken}");

        // Assert
        AssertApiSuccess(response); // Should succeed without authentication
    }

    // Performance test
    [Fact]
    public async Task VerifyEmail_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var userTokenPairs = await Task.WhenAll(
            Enumerable.Range(0, 5)
                .Select(_ => CreateUnverifiedUserWithTokenAsync())
        );

        var tasks = userTokenPairs
            .Select(pair => GetApiResponseAsync<string>($"v1/auth/verify-email/{pair.Token}"))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
    }

    #region Helper Methods

    private async Task<(Guid UserId, string Token)> CreateUnverifiedUserWithTokenAsync()
    {
        var email = $"test-{Guid.NewGuid():N}@example.com";
        var verificationToken = Guid.NewGuid().ToString("N");

        var userId = await ExecuteDbContextAsync(async context =>
        {
            // Create user using domain factory methods to ensure proper initialization
            var emailValueObject = Shopilent.Domain.Identity.ValueObjects.Email.Create(email).Value;
            var fullName = Shopilent.Domain.Identity.ValueObjects.FullName.Create("Test", "User").Value;
            var userResult = Shopilent.Domain.Identity.User.Create(
                emailValueObject, 
                "hash", // In real scenario this would be properly hashed
                fullName
            );

            var user = userResult.Value;
            
            // Generate a verification token and then override it for predictable testing
            user.GenerateEmailVerificationToken();
            
            // Use reflection to set our specific test token since the properties are private setters
            var userType = typeof(Shopilent.Domain.Identity.User);
            var tokenProperty = userType.GetProperty("EmailVerificationToken");
            var tokenField = userType.GetField("<EmailVerificationToken>k__BackingField", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (tokenField != null)
            {
                tokenField.SetValue(user, verificationToken);
            }

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user.Id;
        });

        return (userId, verificationToken);
    }

    private async Task<(Guid UserId, string Token)> CreateUserWithExpiredTokenAsync()
    {
        var email = $"expired-{Guid.NewGuid():N}@example.com";
        var expiredToken = Guid.NewGuid().ToString("N");

        var userId = await ExecuteDbContextAsync(async context =>
        {
            var emailValueObject = Shopilent.Domain.Identity.ValueObjects.Email.Create(email).Value;
            var fullName = Shopilent.Domain.Identity.ValueObjects.FullName.Create("Expired", "User").Value;
            var userResult = Shopilent.Domain.Identity.User.Create(emailValueObject, "hash", fullName);
            var user = userResult.Value;
            
            // Generate token first then override with expired values
            user.GenerateEmailVerificationToken();
            
            var userType = typeof(Shopilent.Domain.Identity.User);
            var tokenField = userType.GetField("<EmailVerificationToken>k__BackingField", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var expiresField = userType.GetField("<EmailVerificationExpires>k__BackingField", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            tokenField?.SetValue(user, expiredToken);
            expiresField?.SetValue(user, DateTime.UtcNow.AddHours(-1)); // Expired 1 hour ago

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user.Id;
        });

        return (userId, expiredToken);
    }

    private async Task<(Guid UserId, string Token)> CreateAlreadyVerifiedUserAsync()
    {
        var email = $"verified-{Guid.NewGuid():N}@example.com";
        var token = Guid.NewGuid().ToString("N");

        var userId = await ExecuteDbContextAsync(async context =>
        {
            var emailValueObject = Shopilent.Domain.Identity.ValueObjects.Email.Create(email).Value;
            var fullName = Shopilent.Domain.Identity.ValueObjects.FullName.Create("Verified", "User").Value;
            var userResult = Shopilent.Domain.Identity.User.CreatePreVerified(emailValueObject, "hash", fullName);
            var user = userResult.Value;
            
            // Generate and set token for testing purposes
            user.GenerateEmailVerificationToken();
            
            var userType = typeof(Shopilent.Domain.Identity.User);
            var tokenField = userType.GetField("<EmailVerificationToken>k__BackingField", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            tokenField?.SetValue(user, token);

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user.Id;
        });

        return (userId, token);
    }

    #endregion
}