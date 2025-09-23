using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Identity.Commands.Register.V1;
using Shopilent.Application.Features.Identity.Commands.ChangeUserRole.V1;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.GetCurrentUserProfile.V1;

public class GetCurrentUserProfileEndpointV1Tests : ApiIntegrationTestBase
{
    public GetCurrentUserProfileEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithAuthenticatedAdmin_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>("v1/users/me");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Email.Should().Be("admin@shopilent.com");
        response.Data.Role.Should().Be(UserRole.Admin);
        response.Data.IsActive.Should().BeTrue();
        response.Data.FirstName.Should().NotBeNullOrEmpty();
        response.Data.LastName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithAuthenticatedCustomer_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>("v1/users/me");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Email.Should().Be("customer@shopilent.com");
        response.Data.Role.Should().Be(UserRole.Customer);
        response.Data.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithManagerRole_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var managerUserId = await CreateTestUserWithRoleAsync("manager@example.com", "Manager", "Test", UserRole.Manager);
        var accessToken = await AuthenticateAsync("manager@example.com", "Password123!");
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>("v1/users/me");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(managerUserId);
        response.Data.Email.Should().Be("manager@example.com");
        response.Data.Role.Should().Be(UserRole.Manager);
        response.Data.FirstName.Should().Be("Manager");
        response.Data.LastName.Should().Be("Test");
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithCustomUser_ShouldReturnCorrectUserData()
    {
        // Arrange
        var testUserData = GetCurrentUserProfileTestDataV1.CreateCustomerUserData();
        var testUserId = await CreateTestUserAsync(
            "custom@example.com",
            "Custom",
            "User",
            "+1234567890");

        var accessToken = await AuthenticateAsync("custom@example.com", "Password123!");
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>("v1/users/me");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(testUserId);
        response.Data.Email.Should().Be("custom@example.com");
        response.Data.FirstName.Should().Be("Custom");
        response.Data.LastName.Should().Be("User");
        response.Data.Phone.Should().Be("+1234567890");
        response.Data.Role.Should().Be(UserRole.Customer);
        response.Data.IsActive.Should().BeTrue();
        response.Data.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task GetCurrentUserProfile_ShouldIncludeUserDetails()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>("v1/users/me");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Addresses.Should().NotBeNull();
        response.Data.RefreshTokens.Should().NotBeNull();
        response.Data.FailedLoginAttempts.Should().BeGreaterThanOrEqualTo(0);
        response.Data.CreatedAt.Should().NotBe(default);
        response.Data.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();

        // Act
        var response = await Client.GetAsync("v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        SetAuthenticationHeader("expired.jwt.token");

        // Act
        var response = await Client.GetAsync("v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithMalformedToken_ShouldReturnUnauthorized()
    {
        // Arrange
        SetAuthenticationHeader("malformed-token");

        // Act
        var response = await Client.GetAsync("v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithInactiveUser_ShouldReturnUserWithInactiveStatus()
    {
        // Arrange - Create user as active first, authenticate, then deactivate
        var userId = await CreateTestUserAsync("inactive@example.com", "Inactive", "User", isActive: true);
        var accessToken = await AuthenticateAsync("inactive@example.com", "Password123!");
        SetAuthenticationHeader(accessToken);

        // Deactivate the user after authentication
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FindAsync(userId);
            user!.Deactivate();
            await context.SaveChangesAsync();
        });

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>("v1/users/me");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(userId);
        response.Data.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithUnicodeCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var unicodeUserId = await CreateTestUserAsync("unicode@example.com", "Ünicöde", "Üser");
        var accessToken = await AuthenticateAsync("unicode@example.com", "Password123!");
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>("v1/users/me");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(unicodeUserId);
        response.Data.FirstName.Should().Be("Ünicöde");
        response.Data.LastName.Should().Be("Üser");
    }

    [Fact]
    public async Task GetCurrentUserProfile_MultipleConsecutiveRequests_ShouldReturnConsistentData()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response1 = await GetApiResponseAsync<UserDetailDto>("v1/users/me");
        var response2 = await GetApiResponseAsync<UserDetailDto>("v1/users/me");
        var response3 = await GetApiResponseAsync<UserDetailDto>("v1/users/me");

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);
        AssertApiSuccess(response3);

        response1!.Data.Id.Should().Be(response2!.Data.Id);
        response2!.Data.Id.Should().Be(response3!.Data.Id);
        response1.Data.Email.Should().Be(response2.Data.Email);
        response2.Data.Email.Should().Be(response3.Data.Email);
    }

    [Fact]
    public async Task GetCurrentUserProfile_ValidRequest_ShouldHaveReasonableResponseTime()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>("v1/users/me");
        stopwatch.Stop();

        // Assert
        AssertApiSuccess(response);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetCurrentUserProfile_DatabaseOperations_ShouldVerifyUserProfileAccuracy()
    {
        // Arrange
        var testUserId = await CreateTestUserAsync("accurate@example.com", "Test", "Accurate", "+9876543210");
        var accessToken = await AuthenticateAsync("accurate@example.com", "Password123!");
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>("v1/users/me");

        // Assert
        AssertApiSuccess(response);

        // Verify the response matches database data
        await ExecuteDbContextAsync(async context =>
        {
            var userFromDb = await context.Users
                .FirstOrDefaultAsync(u => u.Id == testUserId);

            userFromDb.Should().NotBeNull();
            response!.Data.Id.Should().Be(userFromDb!.Id);
            response.Data.Email.Should().Be(userFromDb.Email.Value);
            response.Data.FirstName.Should().Be(userFromDb.FullName.FirstName);
            response.Data.LastName.Should().Be(userFromDb.FullName.LastName);
            response.Data.MiddleName.Should().Be(userFromDb.FullName.MiddleName);
            response.Data.Phone.Should().Be(userFromDb.Phone?.Value ?? string.Empty);
            response.Data.Role.Should().Be(userFromDb.Role);
            response.Data.IsActive.Should().Be(userFromDb.IsActive);
        });
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Manager)]
    [InlineData(UserRole.Customer)]
    public async Task GetCurrentUserProfile_WithDifferentRoles_ShouldReturnCorrectRole(UserRole role)
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var email = $"{role.ToString().ToLower()}role@example.com";
        var userId = await CreateTestUserWithRoleAsync(email, "Test", role.ToString(), role);
        var accessToken = await AuthenticateAsync(email, "Password123!");
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<UserDetailDto>("v1/users/me");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(userId);
        response.Data.Role.Should().Be(role);
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithBearerTokenPrefix_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();

        // The AuthenticateAsAdminAsync already returns token without Bearer prefix
        // SetAuthenticationHeader handles adding Bearer prefix internally
        SetAuthenticationHeader(accessToken);

        // Act
        var httpResponse = await Client.GetAsync("v1/users/me");
        
        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await httpResponse.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        var response = JsonSerializer.Deserialize<ApiResponse<UserDetailDto>>(content, JsonOptions);
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Role.Should().Be(UserRole.Admin);
    }

    // Helper method for creating test users with custom data
    private async Task<Guid> CreateTestUserAsync(
        string email,
        string firstName,
        string lastName,
        string phone = "",
        bool isActive = true)
    {
        using var scope = Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var registerCommand = new RegisterCommandV1
        {
            Email = email,
            Password = "Password123!",
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
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

    // Response DTO for this specific endpoint version
    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool EmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IReadOnlyList<object> Addresses { get; set; } = new List<object>();
        public IReadOnlyList<object> RefreshTokens { get; set; } = new List<object>();
        public int FailedLoginAttempts { get; set; }
        public DateTime? LastFailedAttempt { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
