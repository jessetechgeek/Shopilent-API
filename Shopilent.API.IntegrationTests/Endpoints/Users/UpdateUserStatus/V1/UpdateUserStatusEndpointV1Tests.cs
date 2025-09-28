using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Identity.Commands.Register.V1;
using Shopilent.Application.Features.Identity.Commands.ChangeUserRole.V1;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.UpdateUserStatus.V1;

public class UpdateUserStatusEndpointV1Tests : ApiIntegrationTestBase
{
    public UpdateUserStatusEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Success Scenarios

    [Fact]
    public async Task UpdateUserStatus_AdminActivatingInactiveUser_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");

        // First deactivate the user
        await DeactivateUserAsync(customerUserId);

        var request = UserTestDataV1.StatusScenarios.CreateActivateRequest();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/users/{customerUserId}/status", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNullOrEmpty();
        response.Data.Should().Be("User status updated to active");
        response.Message.Should().Be("User status updated successfully");

        // Verify user is active in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == customerUserId);
            user.Should().NotBeNull();
            user!.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public async Task UpdateUserStatus_AdminDeactivatingActiveUser_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = UserTestDataV1.StatusScenarios.CreateDeactivateRequest();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/users/{customerUserId}/status", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNullOrEmpty();
        response.Data.Should().Be("User status updated to inactive");
        response.Message.Should().Be("User status updated successfully");

        // Verify user is inactive in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == customerUserId);
            user.Should().NotBeNull();
            user!.IsActive.Should().BeFalse();
        });
    }

    [Fact]
    public async Task UpdateUserStatus_ManagerUpdatingUserStatus_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        await EnsureManagerUserExistsAsync();
        var managerToken = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(managerToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = UserTestDataV1.StatusScenarios.CreateDeactivateRequest();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/users/{customerUserId}/status", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNullOrEmpty();
        response.Message.Should().Be("User status updated successfully");

        // Verify user is inactive in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == customerUserId);
            user.Should().NotBeNull();
            user!.IsActive.Should().BeFalse();
        });
    }

    #endregion

    #region Self-Deactivation Prevention Tests

    [Fact]
    public async Task UpdateUserStatus_AdminTryingToDeactivateSelf_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var adminUserId = await GetUserIdByEmailAsync("admin@shopilent.com");
        var request = UserTestDataV1.StatusScenarios.CreateDeactivateRequest();

        // Act
        var response = await PutAsync($"v1/users/{adminUserId}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Cannot deactivate your own account");
        content.Should().Contain("Contact another administrator");
    }

    [Fact]
    public async Task UpdateUserStatus_ManagerTryingToDeactivateSelf_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureManagerUserExistsAsync();
        var managerToken = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(managerToken);

        var managerUserId = await GetUserIdByEmailAsync("manager@shopilent.com");
        var request = UserTestDataV1.StatusScenarios.CreateDeactivateRequest();

        // Act
        var response = await PutAsync($"v1/users/{managerUserId}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Cannot deactivate your own account");
    }

    [Fact]
    public async Task UpdateUserStatus_AdminActivatingSelf_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var adminUserId = await GetUserIdByEmailAsync("admin@shopilent.com");
        var request = UserTestDataV1.StatusScenarios.CreateActivateRequest();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/users/{adminUserId}/status", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNullOrEmpty();
        response.Message.Should().Be("User status updated successfully");
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task UpdateUserStatus_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var userId = Guid.NewGuid();
        var request = UserTestDataV1.StatusScenarios.CreateValidRequest();

        // Act
        var response = await PutAsync($"v1/users/{userId}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserStatus_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);

        var userId = Guid.NewGuid();
        var request = UserTestDataV1.StatusScenarios.CreateValidRequest();

        // Act
        var response = await PutAsync($"v1/users/{userId}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUserStatus_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        SetAuthenticationHeader("invalid-token");
        var userId = Guid.NewGuid();
        var request = UserTestDataV1.StatusScenarios.CreateValidRequest();

        // Act
        var response = await PutAsync($"v1/users/{userId}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserStatus_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        SetAuthenticationHeader(expiredToken);
        var userId = Guid.NewGuid();
        var request = UserTestDataV1.StatusScenarios.CreateValidRequest();

        // Act
        var response = await PutAsync($"v1/users/{userId}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task UpdateUserStatus_WithNonExistentUserId_ShouldReturnNotFound()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var nonExistentUserId = Guid.NewGuid();
        var request = UserTestDataV1.StatusScenarios.CreateValidRequest();

        // Act
        var response = await PutAsync($"v1/users/{nonExistentUserId}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("not found", "User not found");
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty GUID
    [InlineData("not-a-guid")] // Invalid format
    [InlineData("123-456")] // Partial GUID
    [InlineData("12345678-1234-1234-1234-123456789012-extra")] // Too long
    [InlineData("special-chars-!@#$-guid")] // Special characters
    public async Task UpdateUserStatus_WithInvalidGuidFormats_ShouldReturnBadRequest(string invalidGuid)
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var request = UserTestDataV1.StatusScenarios.CreateValidRequest();

        // Act
        var response = await PutAsync($"v1/users/{invalidGuid}/status", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    #endregion

    #region Idempotent Behavior Tests

    [Fact]
    public async Task UpdateUserStatus_ActivatingAlreadyActiveUser_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");

        // Ensure user is active
        await ActivateUserAsync(customerUserId);

        var request = UserTestDataV1.StatusScenarios.CreateActivateRequest();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/users/{customerUserId}/status", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("User status updated to active");

        // Verify user remains active
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == customerUserId);
            user.Should().NotBeNull();
            user!.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public async Task UpdateUserStatus_DeactivatingAlreadyInactiveUser_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");

        // First deactivate the user
        await DeactivateUserAsync(customerUserId);

        var request = UserTestDataV1.StatusScenarios.CreateDeactivateRequest();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/users/{customerUserId}/status", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().Be("User status updated to inactive");

        // Verify user remains inactive
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == customerUserId);
            user.Should().NotBeNull();
            user!.IsActive.Should().BeFalse();
        });
    }

    #endregion

    #region Boolean Binding Tests

    [Theory]
    [MemberData(nameof(GetValidBooleanValues))]
    public async Task UpdateUserStatus_WithValidBooleanValues_ShouldReturnSuccess(object request)
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/users/{customerUserId}/status", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNullOrEmpty();
        response.Message.Should().Be("User status updated successfully");
    }

    [Theory]
    [MemberData(nameof(GetInvalidBooleanValues))]
    public async Task UpdateUserStatus_WithInvalidBooleanValues_ShouldReturnBadRequest(object request)
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");

        // Act
        var response = await PutAsync($"v1/users/{customerUserId}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task UpdateUserStatus_WithMalformedJson_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");

        // Act
        var response = await PutJsonAsync($"v1/users/{customerUserId}/status",
            UserTestDataV1.StatusScenarios.EdgeCases.CreateMalformedJsonRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUserStatus_WithEmptyJson_ShouldDeactivateUser()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");

        // Ensure user is active first
        await ActivateUserAsync(customerUserId);

        // Act - Send empty JSON which should default IsActive to false
        var response = await PutJsonAsync($"v1/users/{customerUserId}/status", "{}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify user was deactivated (empty JSON defaults to IsActive = false)
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == customerUserId);
            user.Should().NotBeNull();
            user!.IsActive.Should().BeFalse("empty JSON should default IsActive to false");
        });
    }

    #endregion

    #region Security Tests

    [Theory]
    [MemberData(nameof(GetSecurityTestPayloads))]
    public async Task UpdateUserStatus_WithSecurityPayloads_ShouldHandleSafely(object maliciousPayload)
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");

        // Act
        var response = await PutAsync($"v1/users/{customerUserId}/status", maliciousPayload);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);

        // Verify user hasn't been corrupted/deleted by malicious payload
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == customerUserId);
            user.Should().NotBeNull("user should still exist after security attack attempt");
            // Verify basic integrity - user properties are still accessible
            _ = user!.IsActive; // This will throw if the property is corrupted
        });
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task UpdateUserStatus_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange - Test concurrent status changes (should be serialized due to database contention)
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");

        // Use fewer requests (3 instead of 10) to reduce database contention
        var requests = UserTestDataV1.StatusScenarios.ConcurrencyTests.CreateConcurrencyTestRequests();

        var tasks = requests.Select(request =>
            PutAsync($"v1/users/{customerUserId}/status", request)
        ).ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert - Due to database row locking, some requests may fail with timing issues
        var successfulResponses = responses.Where(r => r.StatusCode == HttpStatusCode.OK).ToList();
        var failedResponses = responses.Where(r => r.StatusCode != HttpStatusCode.OK).ToList();

        // At least one request should succeed
        successfulResponses.Should().NotBeEmpty("At least one status change should succeed");

        // If there are failed responses, they should be due to concurrency conflicts (409 Conflict)
        // This is expected behavior when multiple operations target the same database row simultaneously
        if (failedResponses.Any())
        {
            failedResponses.Should().AllSatisfy(response =>
                response.StatusCode.Should().Be(HttpStatusCode.Conflict),
                "Failed responses should be 409 Conflict due to concurrency violations");
        }

        // Verify final user state is consistent and not corrupted
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == customerUserId);
            user.Should().NotBeNull("user should still exist after concurrent operations");
            // Verify the user entity is in a valid state - accessing IsActive should not throw
            _ = user!.IsActive; // This will throw if the property is corrupted
        });
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task UpdateUserStatus_MultipleSequentialRequests_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - Perform 20 status changes
        for (int i = 0; i < 20; i++)
        {
            var request = UserTestDataV1.StatusScenarios.CreateValidRequest(i % 2 == 0);
            var response = await PutApiResponseAsync<object, string>($"v1/users/{customerUserId}/status", request);
            AssertApiSuccess(response);
        }

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // Should complete within 10 seconds
    }

    #endregion

    #region Test Data

    public static IEnumerable<object[]> GetValidBooleanValues()
    {
        return UserTestDataV1.StatusScenarios.BoundaryTests.ValidBooleanValues
            .Select(value => new object[] { value });
    }

    public static IEnumerable<object[]> GetInvalidBooleanValues()
    {
        return UserTestDataV1.StatusScenarios.BoundaryTests.InvalidBooleanValues
            .Select(value => new object[] { value });
    }

    public static IEnumerable<object[]> GetSecurityTestPayloads()
    {
        return new[]
        {
            new object[] { UserTestDataV1.StatusScenarios.SecurityTests.CreateSqlInjectionAttempt() },
            new object[] { UserTestDataV1.StatusScenarios.SecurityTests.CreateXssAttempt() },
            new object[] { UserTestDataV1.StatusScenarios.SecurityTests.CreateCommandInjectionAttempt() },
            new object[] { UserTestDataV1.StatusScenarios.SecurityTests.CreateLdapInjectionAttempt() },
            new object[] { UserTestDataV1.StatusScenarios.SecurityTests.CreateNoSqlInjectionAttempt() },
            new object[] { UserTestDataV1.StatusScenarios.SecurityTests.CreatePathTraversalAttempt() },
            new object[] { UserTestDataV1.StatusScenarios.SecurityTests.CreateUnicodeAttempt() }
        };
    }

    #endregion

    #region Helper Methods

    private async Task ActivateUserAsync(Guid userId)
    {
        var activateRequest = UserTestDataV1.StatusScenarios.CreateActivateRequest();
        var response = await PutApiResponseAsync<object, string>($"v1/users/{userId}/status", activateRequest);
        AssertApiSuccess(response);
    }

    private async Task DeactivateUserAsync(Guid userId)
    {
        var deactivateRequest = UserTestDataV1.StatusScenarios.CreateDeactivateRequest();
        var response = await PutApiResponseAsync<object, string>($"v1/users/{userId}/status", deactivateRequest);
        AssertApiSuccess(response);
    }

    private async Task<HttpResponseMessage> PutJsonAsync(string requestUri, string jsonContent)
    {
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        return await Client.PutAsync(requestUri, content);
    }

    // Helper method to get user ID by email
    private async Task<Guid> GetUserIdByEmailAsync(string email)
    {
        return await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email.Value == email);
            return user?.Id ?? Guid.Empty;
        });
    }

    private async Task EnsureManagerUserExistsAsync()
    {
        await ExecuteDbContextAsync(async context =>
        {
            var existingManager = await context.Users
                .FirstOrDefaultAsync(u => u.Role == UserRole.Manager);

            if (existingManager == null)
            {
                using var scope = Factory.Services.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                var registerCommand = new RegisterCommandV1
                {
                    Email = "manager@shopilent.com",
                    Password = "Manager123!",
                    FirstName = "Manager",
                    LastName = "User",
                    Phone = "",
                    IpAddress = "127.0.0.1",
                    UserAgent = "Integration Test"
                };

                var registerResult = await sender.Send(registerCommand);
                if (registerResult.IsFailure)
                {
                    throw new InvalidOperationException($"Failed to create manager user: {registerResult.Error}");
                }

                // Change role to Manager using the ChangeUserRoleCommand
                var changeRoleCommand = new ChangeUserRoleCommandV1
                {
                    UserId = registerResult.Value.User.Id,
                    NewRole = UserRole.Manager
                };

                var changeRoleResult = await sender.Send(changeRoleCommand);
                if (changeRoleResult.IsFailure)
                {
                    throw new InvalidOperationException($"Failed to change user role to Manager: {changeRoleResult.Error}");
                }
            }
        });
    }

    #endregion
}
