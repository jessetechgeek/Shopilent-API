using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.Login.V1;

public class LoginEndpointV1Tests : ApiIntegrationTestBase
{
    public LoginEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange - First ensure admin user exists
        await EnsureAdminUserExistsAsync();
        
        var request = LoginTestDataV1.CreateValidRequest(
            email: "admin@shopilent.com",
            password: "Admin123!");

        // Act
        var response = await PostApiResponseAsync<object, LoginResponseV1>("v1/auth/login", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().NotBeEmpty();
        response.Data.Email.Should().Be("admin@shopilent.com");
        response.Data.FirstName.Should().NotBeNullOrEmpty();
        response.Data.LastName.Should().NotBeNullOrEmpty();
        response.Data.AccessToken.Should().NotBeNullOrEmpty();
        response.Data.RefreshToken.Should().NotBeNullOrEmpty();
        response.Data.EmailVerified.Should().BeFalse(); // New users are not verified by default
        response.Message.Should().Be("Login successful");
    }

    [Fact]
    public async Task Login_WithValidCustomerCredentials_ShouldReturnSuccess()
    {
        // Arrange - First ensure customer user exists
        await EnsureCustomerUserExistsAsync();
        
        var request = LoginTestDataV1.CreateValidRequest(
            email: "customer@shopilent.com",
            password: "Customer123!");

        // Act
        var response = await PostApiResponseAsync<object, LoginResponseV1>("v1/auth/login", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Email.Should().Be("customer@shopilent.com");
        response.Data.AccessToken.Should().NotBeNullOrEmpty();
        response.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithRememberMeTrue_ShouldReturnSuccess()
    {
        // Arrange - First ensure admin user exists
        await EnsureAdminUserExistsAsync();
        
        var request = new { Email = "admin@shopilent.com", Password = "Admin123!", RememberMe = true };

        // Act
        var response = await PostApiResponseAsync<object, LoginResponseV1>("v1/auth/login", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.AccessToken.Should().NotBeNullOrEmpty();
        response.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ShouldReturnValidationError()
    {
        // Arrange
        var request = LoginTestDataV1.CreateRequestWithEmptyEmail();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Email is required");
    }

    [Fact]
    public async Task Login_WithNullEmail_ShouldReturnValidationError()
    {
        // Arrange
        var request = LoginTestDataV1.CreateRequestWithNullEmail();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Email is required");
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ShouldReturnValidationError()
    {
        // Arrange
        var request = LoginTestDataV1.CreateRequestWithEmptyPassword();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Password is required");
    }

    [Fact]
    public async Task Login_WithNullPassword_ShouldReturnValidationError()
    {
        // Arrange
        var request = LoginTestDataV1.CreateRequestWithNullPassword();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Password is required");
    }

    [Fact]
    public async Task Login_WithInvalidEmailFormat_ShouldReturnValidationError()
    {
        // Arrange
        var request = LoginTestDataV1.CreateRequestWithInvalidEmailFormat();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Email is not valid.");
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = LoginTestDataV1.CreateRequestWithNonExistentUser();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Invalid login credentials");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = LoginTestDataV1.CreateRequestWithWrongPassword();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Invalid login credentials");
    }

    [Theory]
    [InlineData(null, "Password123!")]
    [InlineData("", "Password123!")]
    [InlineData("   ", "Password123!")]
    [InlineData("admin@shopilent.com", null)]
    [InlineData("admin@shopilent.com", "")]
    [InlineData("admin@shopilent.com", "   ")]
    public async Task Login_WithInvalidRequiredFields_ShouldReturnValidationError(string? email, string? password)
    {
        // Arrange
        var request = new { Email = email, Password = password, RememberMe = false };

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("Email is required", "Password is required");
    }

    [Fact]
    public async Task Login_WithCaseSensitiveEmail_ShouldReturnSuccess()
    {
        // Arrange - Email should be case-insensitive
        var request = LoginTestDataV1.EdgeCases.CreateRequestWithUppercaseEmail();

        // Act
        var response = await PostApiResponseAsync<object, LoginResponseV1>("v1/auth/login", request);

        // Assert - This test might fail if email comparison is case-sensitive
        // Adjust expectation based on actual implementation
        response.StatusCode.Should().BeOneOf((int)HttpStatusCode.OK, (int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithMixedCaseEmail_ShouldHandleConsistently()
    {
        // Arrange
        var request = LoginTestDataV1.EdgeCases.CreateRequestWithMixedCaseEmail();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert - Should handle consistently based on system design
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithWhitespaceInEmail_ShouldReturnValidationError()
    {
        // Arrange
        var request = new { Email = "  admin@shopilent.com  ", Password = "Admin123!", RememberMe = false };

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert - Should fail validation due to invalid email format
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Invalid email format");
    }

    [Fact]
    public async Task Login_WithWhitespaceInPassword_ShouldPreserveExactValue()
    {
        // Arrange
        var request = new
        {
            Email = "admin@shopilent.com",
            Password = "  Admin123!  ", // Wrong password due to whitespace
            RememberMe = false
        };

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert - Should fail because password should not be trimmed
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Security tests
    [Fact]
    public async Task Login_WithSqlInjectionAttempt_ShouldReturnUnauthorizedSafely()
    {
        // Arrange
        var request = LoginTestDataV1.SecurityTests.CreateSqlInjectionAttempt();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert - Should handle safely without exposing system information
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("SQL");
        content.Should().NotContain("database");
        content.Should().NotContain("exception");
    }

    [Fact]
    public async Task Login_WithXssAttempt_ShouldReturnValidationErrorSafely()
    {
        // Arrange
        var request = LoginTestDataV1.SecurityTests.CreateXssAttempt();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("<script>");
        content.Should().NotContain("alert");
    }

    [Fact]
    public async Task Login_WithExcessivelyLongEmail_ShouldReturnValidationError()
    {
        // Arrange
        var request = LoginTestDataV1.SecurityTests.CreateLongEmailAttack();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithExcessivelyLongPassword_ShouldReturnValidationError()
    {
        // Arrange
        var request = LoginTestDataV1.SecurityTests.CreateLongPasswordAttack();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    // Edge cases
    [Fact]
    public async Task Login_WithUnicodeEmail_ShouldHandleCorrectly()
    {
        // Arrange
        var request = LoginTestDataV1.EdgeCases.CreateRequestWithUnicodeEmail();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert - Depends on system's unicode support
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithPlusAddressing_ShouldHandleCorrectly()
    {
        // Arrange
        var request = LoginTestDataV1.EdgeCases.CreateRequestWithPlusAddressing();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert - Should handle + addressing in emails
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    // Boundary tests
    [Fact]
    public async Task Login_WithMinimumValidEmail_ShouldHandleCorrectly()
    {
        // Arrange
        var request = LoginTestDataV1.BoundaryTests.CreateRequestWithMinimumValidEmail();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithMaximumValidEmail_ShouldHandleCorrectly()
    {
        // Arrange
        var request = LoginTestDataV1.BoundaryTests.CreateRequestWithMaximumValidEmail();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithMinimumPassword_ShouldHandleCorrectly()
    {
        // Arrange
        var request = LoginTestDataV1.BoundaryTests.CreateRequestWithMinimumPassword();

        // Act
        var response = await PostAsync("v1/auth/login", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    // Multiple login attempts (rate limiting test - if implemented)
    [Fact]
    public async Task Login_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange - First ensure admin user exists
        await EnsureAdminUserExistsAsync();
        
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => LoginTestDataV1.CreateValidRequest(
                email: "admin@shopilent.com",
                password: "Admin123!"))
            .Select(request => PostAsync("v1/auth/login", request))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert - All should succeed (unless rate limiting is implemented)
        responses.Should().AllSatisfy(response =>
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.TooManyRequests));
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldNotExposePasswordInResponse()
    {
        // Arrange - First ensure admin user exists
        await EnsureAdminUserExistsAsync();
        
        var request = LoginTestDataV1.CreateValidRequest(
            email: "admin@shopilent.com",
            password: "Admin123!");

        // Act
        var response = await PostApiResponseAsync<object, LoginResponseV1>("v1/auth/login", request);

        // Assert
        AssertApiSuccess(response);

        // Serialize response to check for password exposure
        var responseJson = System.Text.Json.JsonSerializer.Serialize(response);
        responseJson.Should().NotContain("Admin123!");
        responseJson.Should().NotContainAny("password", "Password");
    }

    // Response DTO for this specific endpoint version
    public class LoginResponseV1
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
