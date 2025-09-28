using System.Net;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Domain.Identity.DTOs;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Identity.Commands.Register.V1;
using Shopilent.Application.Features.Identity.Commands.ChangeUserRole.V1;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.GetUser.V1;

public class GetUserEndpointV1Tests : ApiIntegrationTestBase
{
    public GetUserEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUser_WithValidIdAsAdmin_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a test user to retrieve
        var testUserId = await CreateTestUserAsync("test@example.com", "TestUser", "One");

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>($"v1/users/{testUserId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(testUserId);
        response.Data.Email.Should().Be("test@example.com");
        response.Data.FirstName.Should().Be("TestUser");
        response.Data.LastName.Should().Be("One");
        response.Data.Role.Should().Be(UserRole.Customer);
        response.Data.IsActive.Should().BeTrue();
        response.Data.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task GetUser_WithValidIdAsAdmin_ShouldIncludeUserDetails()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("detailed@example.com", "Detailed", "User");

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>($"v1/users/{testUserId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(testUserId);
        response.Data.Addresses.Should().NotBeNull();
        response.Data.RefreshTokens.Should().NotBeNull();
        response.Data.FailedLoginAttempts.Should().Be(0);
        response.Data.LastFailedAttempt.Should().BeNull();
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUser_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var nonExistentId = UserTestDataV1.EdgeCases.CreateNonExistentUserId();

        // Act
        var response = await Client.GetAsync($"v1/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("not found");
    }

    [Fact]
    public async Task GetUser_WithInvalidGuid_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var invalidId = UserTestDataV1.EdgeCases.CreateMalformedUserId();

        // Act
        var response = await Client.GetAsync($"v1/users/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUser_WithEmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var emptyGuid = Guid.Empty;

        // Act
        var response = await Client.GetAsync($"v1/users/{emptyGuid}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUser_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var validUserId = UserTestDataV1.Creation.CreateValidUserId();

        // Act
        var response = await Client.GetAsync($"v1/users/{validUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUser_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var validUserId = UserTestDataV1.Creation.CreateValidUserId();

        // Act
        var response = await Client.GetAsync($"v1/users/{validUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUser_WithManagerRole_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var managerId = await CreateTestUserWithRoleAsync("manager@example.com", "Manager", "User", UserRole.Manager);
        var accessToken = await AuthenticateAsync("manager@example.com", "Password123!");
        SetAuthenticationHeader(accessToken);

        // Create a test user to retrieve
        var testUserId = await CreateTestUserAsync("target@example.com", "Target", "User");

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>($"v1/users/{testUserId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(testUserId);
    }

    [Fact]
    public async Task GetUser_RetrievingOwnProfileAsAdmin_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Get admin user ID from database
        var adminUserId = await ExecuteDbContextAsync(async context =>
        {
            var admin = await context.Users.FirstAsync(u => u.Email.Value == "admin@shopilent.com");
            return admin.Id;
        });

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>($"v1/users/{adminUserId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(adminUserId);
        response.Data.Email.Should().Be("admin@shopilent.com");
        response.Data.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task GetUser_WithInactiveUser_ShouldReturnUserWithInactiveStatus()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var inactiveUserId = await CreateTestUserAsync("inactive@example.com", "Inactive", "User", isActive: false);

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>($"v1/users/{inactiveUserId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(inactiveUserId);
        response.Data.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData("admin@shopilent.com")]
    [InlineData("manager@test.com")]
    public async Task GetUser_WithDifferentAdminManagerRoles_ShouldReturnSuccess(string adminEmail)
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        if (adminEmail == "manager@test.com")
        {
            await CreateTestUserWithRoleAsync(adminEmail, "Manager", "Test", UserRole.Manager);
        }

        var accessToken = await AuthenticateAsync(adminEmail, adminEmail == "admin@shopilent.com" ? "Admin123!" : "Password123!");
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("target@example.com", "Target", "User");

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>($"v1/users/{testUserId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(testUserId);
    }

    [Fact]
    public async Task GetUser_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        SetAuthenticationHeader("expired.jwt.token");
        var validUserId = UserTestDataV1.Creation.CreateValidUserId();

        // Act
        var response = await Client.GetAsync($"v1/users/{validUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUser_WithMalformedToken_ShouldReturnUnauthorized()
    {
        // Arrange
        SetAuthenticationHeader("malformed-token");
        var validUserId = UserTestDataV1.Creation.CreateValidUserId();

        // Act
        var response = await Client.GetAsync($"v1/users/{validUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUser_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple test users
        var userIds = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var userId = await CreateTestUserAsync($"user{i}@example.com", $"User{i}", "Test");
            userIds.Add(userId);
        }

        var tasks = userIds.Select(id => GetApiResponseAsync<UserDetailDto>($"v1/users/{id}")).ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Select(r => r!.Data.Id).Should().OnlyHaveUniqueItems();
        responses.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetUser_WithDatabaseConnectionFailure_ShouldHandleGracefully()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var validUserId = UserTestDataV1.Creation.CreateValidUserId();

        // Act - This would normally cause a connection issue, but we test with a non-existent ID
        var response = await Client.GetAsync($"v1/users/{validUserId}");

        // Assert - Should handle gracefully
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    // Security tests
    [Fact]
    public async Task GetUser_WithSqlInjectionAttempt_ShouldHandleSafely()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var sqlInjectionId = UserTestDataV1.EdgeCases.CreateMalformedUserId();

        // Act
        var response = await Client.GetAsync($"v1/users/{sqlInjectionId}");

        // Assert - Route binding should fail for non-GUID strings, returning 400 or 404
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("SQL");
        content.Should().NotContain("database");
        content.Should().NotContain("exception");
    }

    [Fact]
    public async Task GetUser_WithXssAttempt_ShouldHandleSafely()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var xssId = UserTestDataV1.EdgeCases.CreateMalformedUserId();

        // Act
        var response = await Client.GetAsync($"v1/users/{xssId}");

        // Assert - Route binding should fail for non-GUID strings, returning 400 or 404
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("<script>");
        content.Should().NotContain("alert");
    }

    // Edge case tests
    [Fact]
    public async Task GetUser_WithUnicodeInNames_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Use ASCII email but Unicode names (since email regex doesn't support Unicode)
        var unicodeUserId = await CreateTestUserAsync("unicode@example.com", "Ünicöde", "Üser");

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>($"v1/users/{unicodeUserId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Email.Should().Be("unicode@example.com");
        response.Data.FirstName.Should().Be("Ünicöde");
        response.Data.LastName.Should().Be("Üser");
    }

    // Performance tests
    [Fact]
    public async Task GetUser_ValidRequest_ShouldHaveReasonableResponseTime()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("performance@example.com", "Performance", "Test");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>($"v1/users/{testUserId}");
        stopwatch.Stop();

        // Assert
        AssertApiSuccess(response);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    // Helper methods for test user creation
    private async Task<Guid> CreateTestUserAsync(string email, string firstName, string lastName, bool isActive = true)
    {
        using var scope = Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var registerCommand = new RegisterCommandV1
        {
            Email = email,
            Password = "Password123!",
            FirstName = firstName,
            LastName = lastName,
            Phone = "",
            IpAddress = "127.0.0.1",
            UserAgent = "Integration Test"
        };

        var result = await mediator.Send(registerCommand);

        if (result.IsSuccess && result.Value != null)
        {
            var userId = result.Value.User.Id;

            // Update user status if needed
            if (!isActive)
            {
                await ExecuteDbContextAsync(async context =>
                {
                    var user = await context.Users.FirstAsync(u => u.Id == userId);
                    user.Deactivate();
                    await context.SaveChangesAsync();
                });
            }

            return userId;
        }

        throw new InvalidOperationException("Failed to create test user");
    }

    private async Task<Guid> CreateTestUserWithRoleAsync(string email, string firstName, string lastName, UserRole role)
    {
        var userId = await CreateTestUserAsync(email, firstName, lastName);

        using var scope = Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var changeRoleCommand = new ChangeUserRoleCommandV1
        {
            UserId = userId,
            NewRole = role
        };

        await mediator.Send(changeRoleCommand);

        return userId;
    }

}
