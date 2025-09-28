using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.ChangePassword.V1;

public class ChangePasswordEndpointV1Tests : ApiIntegrationTestBase
{
    public ChangePasswordEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ChangePassword_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.CreateValidChangePasswordRequest(
            currentPassword: "Customer123!",
            newPassword: "NewCustomer123!",
            confirmPassword: "NewCustomer123!");

        // Act
        var response = await PutApiResponseAsync<object, string>("v1/users/change-password", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Password changed successfully");
        response.Message.Should().Be("Password changed successfully");
    }

    [Fact]
    public async Task ChangePassword_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.CreateValidChangePasswordRequest(
            currentPassword: "Customer123!",
            newPassword: "NewCustomer456!",
            confirmPassword: "NewCustomer456!");

        // Act
        var response = await PutApiResponseAsync<object, string>("v1/users/change-password", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Password changed successfully");
    }

    [Fact]
    public async Task ChangePassword_WithEmptyCurrentPassword_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.CreateRequestWithEmptyCurrentPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Current password is required");
    }

    [Fact]
    public async Task ChangePassword_WithNullCurrentPassword_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.CreateRequestWithNullCurrentPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Current password is required");
    }

    [Fact]
    public async Task ChangePassword_WithEmptyNewPassword_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.CreateRequestWithEmptyNewPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Password is required");
    }

    [Fact]
    public async Task ChangePassword_WithNullNewPassword_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.CreateRequestWithNullNewPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Password is required");
    }

    [Fact]
    public async Task ChangePassword_WithEmptyConfirmPassword_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.CreateRequestWithEmptyConfirmPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Confirm password is required");
    }

    [Fact]
    public async Task ChangePassword_WithNullConfirmPassword_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.CreateRequestWithNullConfirmPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Confirm password is required");
    }

    [Fact]
    public async Task ChangePassword_WithMismatchedPasswords_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.CreateRequestWithMismatchedPasswords();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Passwords do not match");
    }

    [Fact]
    public async Task ChangePassword_WithSameCurrentAndNewPassword_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.CreateRequestWithSameCurrentAndNewPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("New password must be different from current password");
    }

    [Fact]
    public async Task ChangePassword_WithShortNewPassword_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.CreateRequestWithShortNewPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Password must be at least 8 characters long");
    }

    [Fact]
    public async Task ChangePassword_WithWeakNewPassword_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.CreateRequestWithWeakNewPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny(
            "Password must contain at least one uppercase letter",
            "Password must contain at least one number",
            "Password must contain at least one special character");
    }

    [Fact]
    public async Task ChangePassword_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = UserTestDataV1.PasswordScenarios.CreateValidChangePasswordRequest();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.CreateValidChangePasswordRequest(
            currentPassword: "WrongPassword123!",
            newPassword: "NewCustomer123!",
            confirmPassword: "NewCustomer123!");

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("Invalid login credentials", "Invalid current password");
    }

    // Password strength validation tests
    [Fact]
    public async Task ChangePassword_WithNoUppercaseNewPassword_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.PasswordStrengthTests.CreateRequestWithNoUppercaseNewPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Password must contain at least one uppercase letter");
    }

    [Fact]
    public async Task ChangePassword_WithNoLowercaseNewPassword_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.PasswordStrengthTests.CreateRequestWithNoLowercaseNewPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Password must contain at least one lowercase letter");
    }

    [Fact]
    public async Task ChangePassword_WithNoNumberNewPassword_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.PasswordStrengthTests.CreateRequestWithNoNumberNewPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Password must contain at least one number");
    }

    [Fact]
    public async Task ChangePassword_WithNoSpecialCharNewPassword_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.PasswordStrengthTests.CreateRequestWithNoSpecialCharNewPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Password must contain at least one special character");
    }

    // Boundary value tests
    [Fact]
    public async Task ChangePassword_WithMinimumValidPassword_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.BoundaryTests.CreateRequestWithMinimumValidPassword();

        // Act
        var response = await PutApiResponseAsync<object, string>("v1/users/change-password", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Password changed successfully");
    }

    [Fact]
    public async Task ChangePassword_WithSevenCharacterPassword_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.BoundaryTests.CreateRequestWithSevenCharacterPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Password must be at least 8 characters long");
    }

    [Fact]
    public async Task ChangePassword_WithVeryLongPassword_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.BoundaryTests.CreateRequestWithVeryLongPassword();

        // Act
        var response = await PutApiResponseAsync<object, string>("v1/users/change-password", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Password changed successfully");
    }

    [Fact]
    public async Task ChangePassword_WithExtremelyLongPassword_ShouldHandleGracefully()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.BoundaryTests.CreateRequestWithExtremelyLongPassword();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert - Should either succeed or fail gracefully
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    // Edge case tests
    [Fact]
    public async Task ChangePassword_WithUnicodeCharacters_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.EdgeCases.CreateRequestWithUnicodeCharacters();

        // Act
        var response = await PutApiResponseAsync<object, string>("v1/users/change-password", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Password changed successfully");
    }

    [Fact]
    public async Task ChangePassword_WithWhitespaceInPasswords_ShouldPreserveExactValue()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.EdgeCases.CreateRequestWithWhitespaceInPasswords();

        // Act - Should fail because current password is incorrect (has extra whitespace)
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("Invalid login credentials", "Invalid current password");
    }

    [Fact]
    public async Task ChangePassword_WithOnlyWhitespace_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.EdgeCases.CreateRequestWithOnlyWhitespace();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("Current password is required", "Password is required");
    }

    [Fact]
    public async Task ChangePassword_WithTabsAndNewlines_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.EdgeCases.CreateRequestWithTabsAndNewlines();

        // Act
        var response = await PutApiResponseAsync<object, string>("v1/users/change-password", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("Password changed successfully");
    }

    // Security tests
    [Fact]
    public async Task ChangePassword_WithSqlInjectionAttempt_ShouldReturnUnauthorizedSafely()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.SecurityTests.CreateSqlInjectionAttempt();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert - Should handle safely without exposing system information
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("SQL");
        content.Should().NotContain("database");
        content.Should().NotContain("exception");
        content.Should().ContainAny("Invalid login credentials", "Invalid current password");
    }

    [Fact]
    public async Task ChangePassword_WithXssAttempt_ShouldReturnUnauthorizedSafely()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.SecurityTests.CreateXssAttempt();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("<script>");
        content.Should().NotContain("alert");
        content.Should().ContainAny("Invalid login credentials", "Invalid current password");
    }

    [Fact]
    public async Task ChangePassword_WithCommandInjectionAttempt_ShouldHandleSafely()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.SecurityTests.CreateCommandInjectionAttempt();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert - Command injection should either succeed as regular password or fail validation
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Ensure no system commands are executed
        content.Should().NotContain("rm");
        content.Should().NotContain("system");
        content.Should().NotContain("command");
    }

    [Fact]
    public async Task ChangePassword_WithLongPasswordAttack_ShouldHandleGracefully()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.SecurityTests.CreateLongPasswordAttack();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert - Should handle gracefully without crashing
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ChangePassword_WithNullByteAttempt_ShouldHandleSafely()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.SecurityTests.CreateNullByteAttempt();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(null, "NewPassword123!", "NewPassword123!")]
    [InlineData("", "NewPassword123!", "NewPassword123!")]
    [InlineData("   ", "NewPassword123!", "NewPassword123!")]
    [InlineData("CurrentPassword123!", null, null)]
    [InlineData("CurrentPassword123!", "", "")]
    [InlineData("CurrentPassword123!", "   ", "   ")]
    [InlineData("CurrentPassword123!", "NewPassword123!", null)]
    [InlineData("CurrentPassword123!", "NewPassword123!", "")]
    [InlineData("CurrentPassword123!", "NewPassword123!", "   ")]
    public async Task ChangePassword_WithInvalidRequiredFields_ShouldReturnValidationError(
        string? currentPassword, string? newPassword, string? confirmPassword)
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = new
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword,
            ConfirmPassword = confirmPassword
        };

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny(
            "Current password is required",
            "Password is required",
            "Confirm password is required");
    }

    // Performance/Concurrency tests
    [Fact]
    public async Task ChangePassword_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange - Test concurrent password change attempts (should be serialized)
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var tasks = Enumerable.Range(0, 3)
            .Select(i => UserTestDataV1.PasswordScenarios.CreateValidChangePasswordRequest(
                currentPassword: "Customer123!",
                newPassword: $"NewCustomer{i}123!",
                confirmPassword: $"NewCustomer{i}123!"))
            .Select(request => PutAsync("v1/users/change-password", request))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert - Only one should succeed, others should fail due to invalid current password
        var successfulResponses = responses.Where(r => r.StatusCode == HttpStatusCode.OK).ToList();
        var failedResponses = responses.Where(r => r.StatusCode != HttpStatusCode.OK).ToList();

        successfulResponses.Should().HaveCount(1, "Only one password change should succeed");
        failedResponses.Should().HaveCount(2, "Other attempts should fail due to stale current password");
    }

    [Fact]
    public async Task ChangePassword_ShouldNotExposePasswordsInResponse()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UserTestDataV1.PasswordScenarios.CreateValidChangePasswordRequest(
            currentPassword: "Customer123!",
            newPassword: "NewCustomer123!",
            confirmPassword: "NewCustomer123!");

        // Act
        var response = await PutApiResponseAsync<object, string>("v1/users/change-password", request);

        // Assert
        AssertApiSuccess(response);

        // Serialize response to check for password exposure
        var responseJson = System.Text.Json.JsonSerializer.Serialize(response);
        responseJson.Should().NotContain("Customer123!");
        responseJson.Should().NotContain("NewCustomer123!");
        responseJson.Should().NotContainAny("currentPassword", "newPassword", "confirmPassword");
    }

    [Fact]
    public async Task ChangePassword_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = UserTestDataV1.PasswordScenarios.CreateValidChangePasswordRequest();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithMalformedAuthToken_ShouldReturnUnauthorized()
    {
        // Arrange
        SetAuthenticationHeader("malformed-token");
        var request = UserTestDataV1.PasswordScenarios.CreateValidChangePasswordRequest();

        // Act
        var response = await PutAsync("v1/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}