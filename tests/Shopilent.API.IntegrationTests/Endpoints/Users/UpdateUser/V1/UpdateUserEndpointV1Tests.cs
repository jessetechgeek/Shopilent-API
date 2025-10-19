using System.Net;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Identity.Commands.Register.V1;
using Shopilent.Application.Features.Identity.Commands.ChangeUserRole.V1;
using Shopilent.Domain.Identity.DTOs;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.UpdateUser.V1;

public class UpdateUserEndpointV1Tests : ApiIntegrationTestBase
{
    public UpdateUserEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task UpdateUser_WithValidDataAsAdmin_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest("Updated", "UserName", "Middle", "+1234567890");

        // Act
        var response = await PutApiResponseAsync<object, UserDto>($"v1/users/{testUserId}", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(testUserId);
        response.Data.FirstName.Should().Be("Updated");
        response.Data.LastName.Should().Be("UserName");
        response.Data.MiddleName.Should().Be("Middle");
        response.Data.Phone.Should().Be("+1234567890");
        response.Data.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateUser_WithValidDataAsAdmin_ShouldUpdateUserInDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest("UpdatedDB", "UserDB", "MiddleDB", "+1987654321");

        // Act
        var response = await PutApiResponseAsync<object, UserDto>($"v1/users/{testUserId}", request);

        // Assert
        AssertApiSuccess(response);

        // Verify user is updated in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == testUserId);
            user.Should().NotBeNull();
            user!.FullName.FirstName.Should().Be("UpdatedDB");
            user.FullName.LastName.Should().Be("UserDB");
            user.FullName.MiddleName.Should().Be("MiddleDB");
            user.Phone!.Value.Should().Be("+1987654321");
            user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        });
    }

    [Fact]
    public async Task UpdateUser_WithValidDataAsManager_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest("Manager", "Updated", "Test");

        // Act
        var response = await PutApiResponseAsync<object, UserDto>($"v1/users/{testUserId}", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("Manager");
        response.Data.LastName.Should().Be("Updated");
        response.Data.MiddleName.Should().Be("Test");
    }

    [Fact]
    public async Task UpdateUser_WithOnlyRequiredFields_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest(
            "John", "Doe", null, null);

        // Act
        var response = await PutApiResponseAsync<object, UserDto>($"v1/users/{testUserId}", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("John");
        response.Data.LastName.Should().Be("Doe");
        response.Data.MiddleName.Should().BeNullOrEmpty();
        response.Data.Phone.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateUser_WithAllFields_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest(
            "John", "Doe", "Michael", "+1234567890");

        // Act
        var response = await PutApiResponseAsync<object, UserDto>($"v1/users/{testUserId}", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("John");
        response.Data.LastName.Should().Be("Doe");
        response.Data.MiddleName.Should().Be("Michael");
        response.Data.Phone.Should().Be("+1234567890");
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateUser_WithInvalidFirstName_ShouldReturnValidationError(string? invalidFirstName)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest(firstName: invalidFirstName);

        // Act
        var response = await PutAsync($"v1/users/{testUserId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("First name is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateUser_WithInvalidLastName_ShouldReturnValidationError(string? invalidLastName)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest(lastName: invalidLastName);

        // Act
        var response = await PutAsync($"v1/users/{testUserId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Last name is required");
    }

    [Fact]
    public async Task UpdateUser_WithTooLongFirstName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.BoundaryTests.CreateRequestWithTooLongNames();

        // Act
        var response = await PutAsync($"v1/users/{testUserId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("First name cannot exceed 50 characters");
    }

    [Fact]
    public async Task UpdateUser_WithTooLongLastName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.BoundaryTests.CreateRequestWithTooLongNames();

        // Act
        var response = await PutAsync($"v1/users/{testUserId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Last name cannot exceed 50 characters");
    }

    [Fact]
    public async Task UpdateUser_WithTooLongMiddleName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.BoundaryTests.CreateRequestWithTooLongNames();

        // Act
        var response = await PutAsync($"v1/users/{testUserId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Middle name cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("invalid-phone")]
    [InlineData("0123456789")]
    [InlineData("abc123")]
    [InlineData("+")]
    [InlineData("123")]
    public async Task UpdateUser_WithInvalidPhoneFormat_ShouldReturnValidationError(string invalidPhone)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest(phone: invalidPhone);

        // Act
        var response = await PutAsync($"v1/users/{testUserId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        // Different validation layers return different messages
        content.Should().ContainAny("Invalid phone number format", "Phone number format is invalid");
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task UpdateUser_WithMaximumLengthNames_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.BoundaryTests.CreateRequestWithMaximumLengthNames();

        // Act
        var response = await PutApiResponseAsync<object, UserDto>($"v1/users/{testUserId}", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().HaveLength(50);
    }

    [Fact]
    public async Task UpdateUser_WithMinimumValidPhone_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.BoundaryTests.CreateRequestWithMinimumValidPhone();

        // Act
        var response = await PutApiResponseAsync<object, UserDto>($"v1/users/{testUserId}", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Phone.Should().Be("+1234567");
    }

    [Fact]
    public async Task UpdateUser_WithMaximumValidPhone_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.BoundaryTests.CreateRequestWithMaximumValidPhone();

        // Act
        var response = await PutApiResponseAsync<object, UserDto>($"v1/users/{testUserId}", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Phone.Should().Be("+123456789012345");
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task UpdateUser_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var testUserId = Guid.NewGuid();
        var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest();

        // Act
        var response = await PutAsync($"v1/users/{testUserId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUser_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange

        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = Guid.NewGuid();
        var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest();

        // Act
        var response = await PutAsync($"v1/users/{testUserId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task UpdateUser_WithNonExistentUserId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var nonExistentUserId = Guid.NewGuid();
        var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest();

        // Act
        var response = await PutAsync($"v1/users/{nonExistentUserId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateUser_WithInvalidGuidFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var invalidGuid = "invalid-guid-format";
        var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest();

        // Act
        var response = await PutAsync($"v1/users/{invalidGuid}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task UpdateUser_WithUnicodeCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.EdgeCases.CreateUpdateUserRequestWithUnicodeCharacters();

        // Act
        var response = await PutApiResponseAsync<object, UserDto>($"v1/users/{testUserId}", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("Café");
        response.Data.LastName.Should().Be("Münchën");
        response.Data.MiddleName.Should().Be("José-María");
        response.Data.Phone.Should().Be("+34123456789");
    }

    [Fact]
    public async Task UpdateUser_WithSpecialCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.EdgeCases.CreateUpdateUserRequestWithSpecialCharacters();

        // Act
        var response = await PutApiResponseAsync<object, UserDto>($"v1/users/{testUserId}", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("Mary-Jane");
        response.Data.LastName.Should().Be("O'Connor");
        response.Data.MiddleName.Should().Be("D'Angelo");
    }

    [Fact]
    public async Task UpdateUser_WithInternationalPhoneNumber_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest(
            "John", "Doe", "Test", "+44123456789");

        // Act
        var response = await PutApiResponseAsync<object, UserDto>($"v1/users/{testUserId}", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Phone.Should().Be("+44123456789");
    }

    [Fact]
    public async Task UpdateUser_WithPhoneNumberWithoutPlus_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testUserId = await CreateTestUserAsync("test@example.com", "Original", "User");
        var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest(
            "John", "Doe", "Test", "1234567890");

        // Act
        var response = await PutApiResponseAsync<object, UserDto>($"v1/users/{testUserId}", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Phone.Should().Be("1234567890");
    }

    [Fact]
    public async Task UpdateUser_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple test users for concurrent updates
        var userIds = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var userId = await CreateTestUserAsync($"test{i}@example.com", $"User{i}", "Test");
            userIds.Add(userId);
        }

        // Create concurrent update tasks
        var tasks = userIds.Select(async (userId, index) =>
        {
            var request = UserTestDataV1.Creation.CreateValidUserUpdateRequest($"Updated{index}", $"User{index}", $"Test{index}");
            return await PutApiResponseAsync<object, UserDto>($"v1/users/{userId}", request);
        }).ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Select(r => r!.Data.Id).Should().OnlyHaveUniqueItems();
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateTestUserAsync(string email, string firstName, string lastName)
    {
        using var scope = Factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var command = new RegisterCommandV1
        {
            Email = email,
            Password = "TestPassword123!",
            FirstName = firstName,
            LastName = lastName
        };

        var result = await sender.Send(command);
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create test user: {result.Error}");
        }

        return result.Value.User.Id;
    }

    #endregion
}
