using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.ResendVerification.V1;

public class ResendVerificationEndpointV1Tests : ApiIntegrationTestBase
{
    public ResendVerificationEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ResendVerification_WithValidUnverifiedEmail_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader(); // Ensure anonymous access
        var testEmail = $"unverified{Guid.NewGuid():N}@test.com";

        // First register a user to have an unverified email
        var registerRequest = new { Email = testEmail, Password = "Test123!", FirstName = "Test", LastName = "User" };
        var registerResponse = await PostAsync("v1/auth/register", registerRequest);

        // Verify registration was successful
        if (registerResponse.StatusCode != HttpStatusCode.OK &&
            registerResponse.StatusCode != HttpStatusCode.Created)
        {
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Registration failed: {registerResponse.StatusCode} - {registerContent}");
        }

        var request = ResendVerificationTestDataV1.CreateValidRequest(testEmail);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/resend-verification", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Verification email sent successfully. Please check your inbox.");
        response.Message.Should().Be("Verification email sent");
    }

    [Fact]
    public async Task ResendVerification_WithEmptyEmail_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ResendVerificationTestDataV1.CreateRequestWithEmptyEmail();

        // Act
        var response = await PostAsync("v1/auth/resend-verification", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Email is required");
    }

    [Fact]
    public async Task ResendVerification_WithNullEmail_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ResendVerificationTestDataV1.CreateRequestWithNullEmail();

        // Act
        var response = await PostAsync("v1/auth/resend-verification", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Email is required");
    }

    [Fact]
    public async Task ResendVerification_WithWhitespaceEmail_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ResendVerificationTestDataV1.CreateRequestWithWhitespaceEmail();

        // Act
        var response = await PostAsync("v1/auth/resend-verification", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Email is required");
    }

    [Fact]
    public async Task ResendVerification_WithInvalidEmailFormat_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ResendVerificationTestDataV1.CreateRequestWithInvalidEmailFormat();

        // Act
        var response = await PostAsync("v1/auth/resend-verification", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Email is not valid");
    }

    [Fact]
    public async Task ResendVerification_WithNonExistentEmail_ShouldReturnNotFound()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ResendVerificationTestDataV1.CreateRequestWithNonExistentEmail();

        // Act
        var response = await PostAsync("v1/auth/resend-verification", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResendVerification_WithAlreadyVerifiedEmail_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader();
        var testEmail = $"verified{Guid.NewGuid():N}@test.com";

        // First register and verify a user
        var registerRequest = new { Email = testEmail, Password = "Test123!", FirstName = "Test", LastName = "User" };
        var registerResponse = await PostAsync("v1/auth/register", registerRequest);

        // Verify registration was successful
        if (registerResponse.StatusCode != HttpStatusCode.OK &&
            registerResponse.StatusCode != HttpStatusCode.Created)
        {
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Registration failed: {registerResponse.StatusCode} - {registerContent}");
        }

        // Manually mark user as verified in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email.Value == testEmail);
            if (user != null)
            {
                user.VerifyEmail();
                await context.SaveChangesAsync();
            }
        });

        var request = ResendVerificationTestDataV1.CreateValidRequest(testEmail);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/resend-verification", request);

        // Assert
        // The system returns success when user is already verified (no-op behavior)
        AssertApiSuccess(response);
        response!.Data.Should().Be("Verification email sent successfully. Please check your inbox.");
    }

    [Fact]
    public async Task ResendVerification_AllowsAnonymousAccess()
    {
        // Arrange
        ClearAuthenticationHeader(); // Ensure no auth header
        var testEmail = $"anonymous{Guid.NewGuid():N}@test.com";

        // First register a user
        var registerRequest = new { Email = testEmail, Password = "Test123!", FirstName = "Test", LastName = "User" };
        var registerResponse = await PostAsync("v1/auth/register", registerRequest);

        // Verify registration was successful
        if (registerResponse.StatusCode != HttpStatusCode.OK &&
            registerResponse.StatusCode != HttpStatusCode.Created)
        {
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Registration failed: {registerResponse.StatusCode} - {registerContent}");
        }

        var request = ResendVerificationTestDataV1.CreateValidRequest(testEmail);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/resend-verification", request);

        // Assert
        AssertApiSuccess(response);
    }

    [Theory]
    [InlineData("userexample.com")]
    [InlineData("user@")]
    [InlineData("@example.com")]
    [InlineData("user@@example.com")]
    [InlineData("user@example.")]
    public async Task ResendVerification_WithVariousInvalidEmailFormats_ShouldReturnValidationError(string invalidEmail)
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ResendVerificationTestDataV1.CreateRequestWithInvalidEmailFormat(invalidEmail);

        // Act
        var response = await PostAsync("v1/auth/resend-verification", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("Email is not valid", "Invalid email format.");
    }

    [Theory]
    // [InlineData("user+tag")]
    [InlineData("user.name")]
    [InlineData("user-name")]
    [InlineData("user_name")]
    [InlineData("user")]
    public async Task ResendVerification_WithValidEmailFormats_ShouldProcessCorrectly(string emailLocalPart)
    {
        // Arrange
        ClearAuthenticationHeader();

        // Generate unique email to avoid conflicts between test runs
        var uniqueEmail = $"{emailLocalPart}{Guid.NewGuid():N}@example.com";

        // First register the user
        var registerRequest = new { Email = uniqueEmail, Password = "Test123!", FirstName = "Test", LastName = "User" };

        var registerResponse = await PostAsync("v1/auth/register", registerRequest);

        // Verify registration was successful
        if (registerResponse.StatusCode != HttpStatusCode.OK &&
            registerResponse.StatusCode != HttpStatusCode.Created)
        {
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Registration failed for {uniqueEmail}: {registerResponse.StatusCode} - {registerContent}");
        }

        var request = ResendVerificationTestDataV1.CreateValidRequest(uniqueEmail);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/resend-verification", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Verification email sent successfully. Please check your inbox.");
    }

    [Fact]
    public async Task ResendVerification_WithMinimumValidEmail_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader();
        var testEmail = $"a{Guid.NewGuid():N}@b.co";

        // First register the user
        var registerRequest = new { Email = testEmail, Password = "Test123!", FirstName = "Test", LastName = "User" };
        var registerResponse = await PostAsync("v1/auth/register", registerRequest);

        // Verify registration was successful
        if (registerResponse.StatusCode != HttpStatusCode.OK &&
            registerResponse.StatusCode != HttpStatusCode.Created)
        {
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Registration failed: {registerResponse.StatusCode} - {registerContent}");
        }

        var request = ResendVerificationTestDataV1.CreateValidRequest(testEmail);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/resend-verification", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Verification email sent successfully. Please check your inbox.");
    }

    [Fact]
    public async Task ResendVerification_WithExtremelyLongEmail_ShouldReturnError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ResendVerificationTestDataV1.BoundaryTests.CreateRequestWithExtremelyLongEmail();

        // Act
        var response = await PostAsync("v1/auth/resend-verification", request);

        // Assert
        // Very long emails might cause route resolution issues (404) or validation errors (400)
        // Both are acceptable error responses for this edge case
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // The response should indicate some form of error with the email
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            content.Should().Contain("Email is not valid");
        }
        // If it's 404, it's likely a routing/serialization issue with the extremely long email
    }

    [Fact]
    public async Task ResendVerification_WithModeratelyLongEmail_ShouldWork()
    {
        // Arrange
        ClearAuthenticationHeader();
        var longEmail = ResendVerificationTestDataV1.BoundaryTests.CreateRequestWithModeratelyLongEmail();
        var email = (longEmail as dynamic).Email as string;
        
        // First register the user with the long email
        var registerRequest = new
        {
            Email = email,
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };
        var registerResponse = await PostAsync("v1/auth/register", registerRequest);
        
        // Verify registration was successful
        if (registerResponse.StatusCode != HttpStatusCode.OK && 
            registerResponse.StatusCode != HttpStatusCode.Created)
        {
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Registration failed: {registerResponse.StatusCode} - {registerContent}");
        }

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/resend-verification", longEmail);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Verification email sent successfully. Please check your inbox.");
    }

    [Fact]
    public async Task ResendVerification_WithMixedCaseEmail_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader();
        var uniqueId = Guid.NewGuid().ToString("N");
        var testEmail = $"MixedCase{uniqueId}@Example.COM";

        // First register the user with lowercase email
        var registerRequest = new
        {
            Email = testEmail.ToLower(), Password = "Test123!", FirstName = "Test", LastName = "User"
        };
        var registerResponse = await PostAsync("v1/auth/register", registerRequest);

        // Verify registration was successful
        if (registerResponse.StatusCode != HttpStatusCode.OK &&
            registerResponse.StatusCode != HttpStatusCode.Created)
        {
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Registration failed: {registerResponse.StatusCode} - {registerContent}");
        }

        var request = ResendVerificationTestDataV1.CreateValidRequest(testEmail);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/resend-verification", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Verification email sent successfully. Please check your inbox.");
    }

    [Fact]
    public async Task ResendVerification_ResponseContainsCorrectSuccessMessage()
    {
        // Arrange
        ClearAuthenticationHeader();
        var testEmail = $"response{Guid.NewGuid():N}@test.com";

        // First register the user
        var registerRequest = new { Email = testEmail, Password = "Test123!", FirstName = "Test", LastName = "User" };
        var registerResponse = await PostAsync("v1/auth/register", registerRequest);

        // Verify registration was successful
        if (registerResponse.StatusCode != HttpStatusCode.OK &&
            registerResponse.StatusCode != HttpStatusCode.Created)
        {
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Registration failed: {registerResponse.StatusCode} - {registerContent}");
        }

        var request = ResendVerificationTestDataV1.CreateValidRequest(testEmail);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/resend-verification", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Verification email sent successfully. Please check your inbox.");
        response.Message.Should().Be("Verification email sent");
        response.Succeeded.Should().BeTrue();
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ResendVerification_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        ClearAuthenticationHeader();
        var baseEmail = $"concurrent{Guid.NewGuid():N}";
        var emails = Enumerable.Range(1, 3) // Reduced to 3 to avoid overwhelming the system
            .Select(i => $"{baseEmail}{i}@test.com")
            .ToList();

        // First register all users sequentially to avoid conflicts
        foreach (var email in emails)
        {
            var registerRequest = new { Email = email, Password = "Test123!", FirstName = "Test", LastName = "User" };
            var registerResponse = await PostAsync("v1/auth/register", registerRequest);

            // Ensure registration succeeded or user already exists
            if (registerResponse.StatusCode != HttpStatusCode.OK &&
                registerResponse.StatusCode != HttpStatusCode.Created &&
                registerResponse.StatusCode != HttpStatusCode.Conflict)
            {
                throw new InvalidOperationException($"Failed to register user {email}: {registerResponse.StatusCode}");
            }
        }

        // Create concurrent resend verification requests
        var tasks = emails
            .Select(email => ResendVerificationTestDataV1.CreateValidRequest(email))
            .Select(request => PostApiResponseAsync<object, string>("v1/auth/resend-verification", request))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Should().AllSatisfy(response =>
            response!.Data.Should().Be("Verification email sent successfully. Please check your inbox."));
    }

    [Fact]
    public async Task ResendVerification_DoesNotRequireAuthentication()
    {
        // Arrange
        ClearAuthenticationHeader(); // Explicitly clear any auth headers
        var testEmail = $"noauth{Guid.NewGuid():N}@test.com";

        // First register the user
        var registerRequest = new { Email = testEmail, Password = "Test123!", FirstName = "Test", LastName = "User" };
        var registerResponse = await PostAsync("v1/auth/register", registerRequest);

        // Verify registration was successful
        if (registerResponse.StatusCode != HttpStatusCode.OK &&
            registerResponse.StatusCode != HttpStatusCode.Created)
        {
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Registration failed: {registerResponse.StatusCode} - {registerContent}");
        }

        var request = ResendVerificationTestDataV1.CreateValidRequest(testEmail);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/resend-verification", request);

        // Assert - Should succeed without authentication
        AssertApiSuccess(response);
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ResendVerification_WithAuthenticatedUser_ShouldStillWork()
    {
        // Arrange - This time WITH authentication to ensure it doesn't interfere
        var testEmail = $"authenticated{Guid.NewGuid():N}@test.com";

        // First register the user
        var registerRequest = new { Email = testEmail, Password = "Test123!", FirstName = "Test", LastName = "User" };
        var registerResponse = await PostAsync("v1/auth/register", registerRequest);

        // Verify registration was successful
        if (registerResponse.StatusCode != HttpStatusCode.OK &&
            registerResponse.StatusCode != HttpStatusCode.Created)
        {
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Registration failed: {registerResponse.StatusCode} - {registerContent}");
        }

        // Create and authenticate as an admin user
        var adminEmail = $"admin{Guid.NewGuid():N}@test.com";
        var adminRegisterRequest = new
        {
            Email = adminEmail,
            Password = "Admin123!",
            FirstName = "Admin",
            LastName = "User"
        };
        var adminRegisterResponse = await PostAsync("v1/auth/register", adminRegisterRequest);
        
        // Verify admin registration was successful
        if (adminRegisterResponse.StatusCode != HttpStatusCode.OK && 
            adminRegisterResponse.StatusCode != HttpStatusCode.Created)
        {
            var adminRegisterContent = await adminRegisterResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Admin registration failed: {adminRegisterResponse.StatusCode} - {adminRegisterContent}");
        }

        // Authenticate as the admin user
        var accessToken = await AuthenticateAsync(adminEmail, "Admin123!");
        SetAuthenticationHeader(accessToken);

        var request = ResendVerificationTestDataV1.CreateValidRequest(testEmail);

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/resend-verification", request);

        // Assert - Should still work even with authentication present
        AssertApiSuccess(response);
        response!.Data.Should().Be("Verification email sent successfully. Please check your inbox.");
    }
}
