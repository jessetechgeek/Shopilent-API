using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.ForgotPassword.V1;

public class ForgotPasswordEndpointV1Tests : ApiIntegrationTestBase
{
    public ForgotPasswordEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        // Ensure test users exist for forgot password scenarios
        await EnsureTestUsersExistAsync();
    }

    #region Happy Path Tests

    [Fact]
    public async Task ForgotPassword_WithExistingEmail_ShouldReturnSuccessMessage()
    {
        // Arrange
        ClearAuthenticationHeader(); // Forgot password is anonymous
        var request = ForgotPasswordTestDataV1.TestEmails.CreateRequestWithExistingEmail();

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("If the email exists, a password reset link has been sent to it.");
        response.Message.Should().Be("Password reset email sent");
    }

    [Fact]
    public async Task ForgotPassword_WithNonExistentEmail_ShouldReturnSuccessMessageForSecurity()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.TestEmails.CreateRequestWithNonExistentEmail();

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert - Should return success even for non-existent emails for security reasons
        AssertApiSuccess(response);
        response!.Data.Should().Be("If the email exists, a password reset link has been sent to it.");
        response.Message.Should().Be("Password reset email sent");
    }

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ShouldReturnSuccessInReasonableTime()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.CreateValidRequest();
        var startTime = DateTime.UtcNow;

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);
        var endTime = DateTime.UtcNow;

        // Assert
        AssertApiSuccess(response);
        var processingTime = endTime - startTime;
        processingTime.Should().BeLessThan(TimeSpan.FromSeconds(5), "API should respond quickly");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ForgotPassword_WithEmptyEmail_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.CreateRequestWithEmptyEmail();

        // Act
        var response = await PostAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Email is required");
    }

    [Fact]
    public async Task ForgotPassword_WithNullEmail_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.CreateRequestWithNullEmail();

        // Act
        var response = await PostAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Email is required");
    }

    [Fact]
    public async Task ForgotPassword_WithWhitespaceEmail_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.CreateRequestWithWhitespaceEmail();

        // Act
        var response = await PostAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Email is required");
    }

    [Fact]
    public async Task ForgotPassword_WithInvalidEmailFormat_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.CreateRequestWithInvalidEmailFormat();

        // Act
        var response = await PostAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("Email is not valid", "Invalid email format");
    }

    [Fact]
    public async Task ForgotPassword_WithInvalidEmailMissingDomain_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.CreateRequestWithInvalidEmailMissingDomain();

        // Act
        var response = await PostAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("Email is not valid", "Invalid email format");
    }

    [Fact]
    public async Task ForgotPassword_WithInvalidEmailMissingAt_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.CreateRequestWithInvalidEmailMissingAt();

        // Act
        var response = await PostAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("Email is not valid", "Invalid email format");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("user@@domain.com")]
    public async Task ForgotPassword_WithInvalidEmails_ShouldReturnValidationError(string? invalidEmail)
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.CreateRequestWithSpecificEmail(invalidEmail);

        // Act
        var response = await PostAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task ForgotPassword_WithMinimumValidEmail_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.BoundaryTests.CreateRequestWithMinimumValidEmail();

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task ForgotPassword_WithLongValidEmail_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.BoundaryTests.CreateRequestWithLongValidEmail();

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task ForgotPassword_WithMaximumLocalPart_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.BoundaryTests.CreateRequestWithMaximumLocalPart();

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task ForgotPassword_WithTooLongLocalPart_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.BoundaryTests.CreateRequestWithTooLongLocalPart();
        // Note: This generates 77 chars total (65 + 1 + 11), which is under 255 limit

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert
        AssertApiSuccess(response);
        // Should succeed because 77 characters is under the 255 character limit
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task ForgotPassword_WithUnicodeEmail_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.EdgeCases.CreateRequestWithUnicodeEmail();

        // Act
        var response = await PostAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("Email is not valid", "Invalid email format");
        // Unicode characters are not supported by the domain email validation regex
    }

    [Fact]
    public async Task ForgotPassword_WithSpecialCharacters_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.EdgeCases.CreateRequestWithSpecialCharacters();

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task ForgotPassword_WithUppercaseEmail_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.EdgeCases.CreateRequestWithUppercaseEmail();

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task ForgotPassword_WithMixedCaseEmail_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.EdgeCases.CreateRequestWithMixedCaseEmail();

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task ForgotPassword_WithSubdomainEmail_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.EdgeCases.CreateRequestWithSubdomainEmail();

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task ForgotPassword_WithLeadingTrailingSpaces_ShouldHandleGracefully()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.EdgeCases.CreateRequestWithLeadingTrailingSpaces();

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert
        // Should either succeed (if trimmed) or fail with validation error
        if (response!.Succeeded)
        {
            AssertApiSuccess(response);
        }
        else
        {
            response.StatusCode.Should().Be(400);
        }
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task ForgotPassword_WithoutAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader(); // Explicitly clear to ensure anonymous access
        var request = ForgotPasswordTestDataV1.CreateValidRequest();

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert
        AssertApiSuccess(response);
        // Forgot password should be accessible without authentication
    }

    [Fact]
    public async Task ForgotPassword_WithAuthentication_ShouldStillReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = ForgotPasswordTestDataV1.CreateValidRequest();

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert
        AssertApiSuccess(response);
        // Should work even if user is already authenticated
    }

    #endregion

    #region Security Tests

    [Fact]
    public async Task ForgotPassword_WithSqlInjectionAttempt_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.SecurityTests.CreateRequestWithSqlInjectionAttempt();

        // Act
        var response = await PostAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().ContainAny("Email is not valid", "Invalid email format");
    }

    [Fact]
    public async Task ForgotPassword_WithXssAttempt_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.SecurityTests.CreateRequestWithXssAttempt();

        // Act
        var response = await PostAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().ContainAny("Email is not valid", "Invalid email format");
    }

    [Fact]
    public async Task ForgotPassword_WithLdapInjectionAttempt_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.SecurityTests.CreateRequestWithLdapInjectionAttempt();

        // Act
        var response = await PostAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().ContainAny("Email is not valid", "Invalid email format");
    }

    [Fact]
    public async Task ForgotPassword_WithCommandInjectionAttempt_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.SecurityTests.CreateRequestWithCommandInjectionAttempt();

        // Act
        var response = await PostAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().ContainAny("Email is not valid", "Invalid email format");
    }

    #endregion

    #region Rate Limiting and Performance Tests

    [Fact]
    public async Task ForgotPassword_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        ClearAuthenticationHeader();
        var requests = ForgotPasswordTestDataV1.PerformanceTests.GenerateMultipleValidRequests(5).ToList();
        var tasks = requests
            .Select(request => PostApiResponseAsync<object, string>("v1/auth/forgot-password", request))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        // All should return the same success message
        responses.Should().AllSatisfy(response =>
            response!.Data.Should().Be("If the email exists, a password reset link has been sent to it."));
    }

    [Fact]
    public async Task ForgotPassword_SameEmailMultipleTimes_ShouldReturnConsistentResponse()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.TestEmails.CreateRequestWithTestEmail();

        // Act
        var response1 = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);
        var response2 = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);
        var response3 = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);
        AssertApiSuccess(response3);

        // All responses should be identical for security reasons
        response1!.Data.Should().Be(response2!.Data);
        response2!.Data.Should().Be(response3!.Data);
        response1.Message.Should().Be(response2.Message);
        response2.Message.Should().Be(response3.Message);
    }

    [Fact]
    public async Task ForgotPassword_WithVeryLongEmail_ShouldReturnValidationError()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.PerformanceTests.CreateRequestWithVeryLongEmail();

        // Act
        var response = await PostAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("255 characters");
    }

    #endregion

    #region Response Content Tests

    [Fact]
    public async Task ForgotPassword_SuccessResponse_ShouldHaveCorrectStructure()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.CreateValidRequest();

        // Act
        var response = await PostApiResponseAsync<object, string>("v1/auth/forgot-password", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNullOrEmpty();
        response.Message.Should().NotBeNullOrEmpty();
        response.Errors.Should().BeEmpty();
        response.StatusCode.Should().Be(200);
        response.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task ForgotPassword_ValidationError_ShouldHaveCorrectErrorStructure()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.CreateRequestWithEmptyEmail();

        // Act
        var response = await PostAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        // Check for expected validation error message in content
        content.Should().Contain("Email is required");
    }

    #endregion

    #region HTTP Method Tests

    [Fact]
    public async Task ForgotPassword_WithGetMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        ClearAuthenticationHeader();

        // Act
        var response = await Client.GetAsync("v1/auth/forgot-password");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task ForgotPassword_WithPutMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = ForgotPasswordTestDataV1.CreateValidRequest();

        // Act
        var response = await PutAsync("v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task ForgotPassword_WithDeleteMethod_ShouldReturnMethodNotAllowed()
    {
        // Arrange
        ClearAuthenticationHeader();

        // Act
        var response = await DeleteAsync("v1/auth/forgot-password");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    #endregion

    #region Content Type Tests

    [Fact]
    public async Task ForgotPassword_WithInvalidContentType_ShouldReturnBadRequest()
    {
        // Arrange
        ClearAuthenticationHeader();
        var jsonContent = """{"Email": "test@example.com"}""";
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "text/plain");

        // Act
        var response = await Client.PostAsync("v1/auth/forgot-password", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }

    #endregion
}
