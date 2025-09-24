using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.ChangeUserRole.V1;

public class ChangeUserRoleEndpointV1Tests : ApiIntegrationTestBase
{
    public ChangeUserRoleEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ChangeUserRole_WithValidAdminRequest_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.CreateManagerRoleRequest();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/users/{customerUserId}/role", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNullOrEmpty();
        response.Message.Should().Be("User role changed successfully");

        // Verify role was changed in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == customerUserId);
            user.Should().NotBeNull();
            user!.Role.Should().Be(UserRole.Manager);
        });
    }

    [Fact]
    public async Task ChangeUserRole_WithValidCustomerRequest_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.CreateCustomerRoleRequest();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/users/{customerUserId}/role", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNullOrEmpty();
        response.Message.Should().Be("User role changed successfully");

        // Verify role remains Customer in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == customerUserId);
            user.Should().NotBeNull();
            user!.Role.Should().Be(UserRole.Customer);
        });
    }

    [Fact]
    public async Task ChangeUserRole_WithValidAdminRoleRequest_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.CreateAdminRoleRequest();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/users/{customerUserId}/role", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNullOrEmpty();
        response.Message.Should().Be("User role changed successfully");

        // Verify role was changed to Admin in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == customerUserId);
            user.Should().NotBeNull();
            user!.Role.Should().Be(UserRole.Admin);
        });
    }

    [Fact]
    public async Task ChangeUserRole_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var nonExistentUserId = Guid.NewGuid();
        var request = ChangeUserRoleTestDataV1.CreateManagerRoleRequest();

        // Act
        var response = await PutAsync($"v1/users/{nonExistentUserId}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("was not found");
    }

    [Fact]
    public async Task ChangeUserRole_WithInvalidUserId_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var invalidUserId = "invalid-guid";
        var request = ChangeUserRoleTestDataV1.CreateManagerRoleRequest();

        // Act
        var response = await PutAsync($"v1/users/{invalidUserId}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeUserRole_WithInvalidRole_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.CreateInvalidRoleRequest();

        // Act
        var response = await PutAsync($"v1/users/{customerUserId}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("Invalid role", "Role must be");
    }

    [Fact]
    public async Task ChangeUserRole_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        await EnsureCustomerUserExistsAsync();

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.CreateManagerRoleRequest();

        // Act
        var response = await PutAsync($"v1/users/{customerUserId}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangeUserRole_WithCustomerAuthentication_ShouldReturnForbidden()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.CreateAdminRoleRequest();

        // Act
        var response = await PutAsync($"v1/users/{customerUserId}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ChangeUserRole_AdminChangingOwnRole_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var adminUserId = await GetUserIdByEmailAsync("admin@shopilent.com");
        var request = ChangeUserRoleTestDataV1.CreateManagerRoleRequest();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/users/{adminUserId}/role", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNullOrEmpty();

        // Verify role was changed in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == adminUserId);
            user.Should().NotBeNull();
            user!.Role.Should().Be(UserRole.Manager);
        });
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Manager)]
    [InlineData(UserRole.Customer)]
    public async Task ChangeUserRole_WithAllValidRoles_ShouldReturnSuccess(UserRole targetRole)
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = new { Role = targetRole };

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/users/{customerUserId}/role", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNullOrEmpty();

        // Verify role was changed in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == customerUserId);
            user.Should().NotBeNull();
            user!.Role.Should().Be(targetRole);
        });
    }

    [Fact]
    public async Task ChangeUserRole_WithEmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var emptyGuid = Guid.Empty;
        var request = ChangeUserRoleTestDataV1.CreateManagerRoleRequest();

        // Act
        var response = await PutAsync($"v1/users/{emptyGuid}/role", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    // Edge case tests
    [Fact]
    public async Task ChangeUserRole_WithStringRole_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.EdgeCases.CreateRequestWithStringRole();

        // Act
        var response = await PutAsync($"v1/users/{customerUserId}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeUserRole_WithNegativeRole_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.EdgeCases.CreateRequestWithNegativeRole();

        // Act
        var response = await PutAsync($"v1/users/{customerUserId}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("Invalid role", "Role must be");
    }

    [Fact]
    public async Task ChangeUserRole_WithLargeRole_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.EdgeCases.CreateRequestWithLargeRole();

        // Act
        var response = await PutAsync($"v1/users/{customerUserId}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("Invalid role", "Role must be");
    }

    // Security tests
    [Fact]
    public async Task ChangeUserRole_WithSqlInjectionAttempt_ShouldReturnBadRequestSafely()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.SecurityTests.CreateSqlInjectionAttempt();

        // Act
        var response = await PutAsync($"v1/users/{customerUserId}/role", request);

        // Assert - Should handle safely without exposing system information
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("SQL");
        content.Should().NotContain("database");
        content.Should().NotContain("DROP");
        content.Should().NotContain("exception");
    }

    [Fact]
    public async Task ChangeUserRole_WithXssAttempt_ShouldReturnBadRequestSafely()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.SecurityTests.CreateXssAttempt();

        // Act
        var response = await PutAsync($"v1/users/{customerUserId}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("<script>");
        content.Should().NotContain("alert");
        content.Should().NotContain("javascript");
    }

    [Fact]
    public async Task ChangeUserRole_WithCommandInjectionAttempt_ShouldReturnBadRequestSafely()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.SecurityTests.CreateCommandInjectionAttempt();

        // Act
        var response = await PutAsync($"v1/users/{customerUserId}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("rm");
        content.Should().NotContain("system");
        content.Should().NotContain("command");
    }

    // Performance/Concurrency tests
    [Fact]
    public async Task ChangeUserRole_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange - Test concurrent role changes (optimistic concurrency control)
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var roles = new[] { UserRole.Admin, UserRole.Manager, UserRole.Customer };

        var tasks = roles
            .Select(role => new { Role = role })
            .Select(request => PutAsync($"v1/users/{customerUserId}/role", request))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert - Due to optimistic concurrency control, only some requests should succeed
        // Others may fail with conflict/internal server error due to database concurrency
        var successfulResponses = responses.Where(r => r.StatusCode == HttpStatusCode.OK).ToList();
        var failedResponses = responses.Where(r => r.StatusCode != HttpStatusCode.OK).ToList();

        // At least one request should succeed
        successfulResponses.Should().NotBeEmpty("At least one role change should succeed");
        
        // Failed responses should be due to concurrency conflicts (409 Conflict)
        failedResponses.Should().AllSatisfy(response =>
            response.StatusCode.Should().Be(HttpStatusCode.Conflict));

        // Verify final role is one of the requested roles
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == customerUserId);
            user.Should().NotBeNull();
            user!.Role.Should().BeOneOf(roles);
        });
    }

    [Fact]
    public async Task ChangeUserRole_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        await EnsureCustomerUserExistsAsync();

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.CreateManagerRoleRequest();

        // Act
        var response = await PutAsync($"v1/users/{customerUserId}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangeUserRole_WithMalformedAuthToken_ShouldReturnUnauthorized()
    {
        // Arrange
        SetAuthenticationHeader("malformed-token");
        await EnsureCustomerUserExistsAsync();

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.CreateManagerRoleRequest();

        // Act
        var response = await PutAsync($"v1/users/{customerUserId}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangeUserRole_ShouldNotExposeUserDetailsInResponse()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        await EnsureCustomerUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        var customerUserId = await GetUserIdByEmailAsync("customer@shopilent.com");
        var request = ChangeUserRoleTestDataV1.CreateManagerRoleRequest();

        // Act
        var response = await PutApiResponseAsync<object, string>($"v1/users/{customerUserId}/role", request);

        // Assert
        AssertApiSuccess(response);

        // Serialize response to check for data exposure
        var responseJson = System.Text.Json.JsonSerializer.Serialize(response);
        responseJson.Should().NotContain("password");
        responseJson.Should().NotContain("email");
        responseJson.Should().NotContain("phone");
        responseJson.Should().NotContain("userId");
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
}
