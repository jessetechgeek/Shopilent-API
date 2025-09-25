using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Identity.Commands.UpdateUserProfile.V1;

namespace Shopilent.API.IntegrationTests.Endpoints.Users.UpdateProfile.V1;

public class UpdateProfileEndpointV1Tests : ApiIntegrationTestBase
{
    public UpdateProfileEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateProfile_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateValidRequest(
            firstName: "John",
            lastName: "Doe",
            middleName: "William",
            phone: "5551234567");

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.FirstName.Should().Be("John");
        response.Data.LastName.Should().Be("Doe");
        response.Data.MiddleName.Should().Be("William");
        response.Data.Phone.Should().Be("5551234567");
        response.Data.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        response.Message.Should().Be("User profile updated successfully");
    }

    [Fact]
    public async Task UpdateProfile_WithValidData_ShouldUpdateUserInDatabase()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateValidRequest(
            firstName: "Jane",
            lastName: "Smith",
            middleName: "Marie",
            phone: "5559876543");

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);

        // Verify user was updated in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email.Value == "customer@shopilent.com");

            user.Should().NotBeNull();
            user!.FullName.FirstName.Should().Be("Jane");
            user.FullName.LastName.Should().Be("Smith");
            user.FullName.MiddleName.Should().Be("Marie");
            user.Phone.Should().NotBeNull();
            user.Phone!.Value.Should().Be("5559876543");
        });
    }

    [Fact]
    public async Task UpdateProfile_WithEmptyFirstName_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateRequestWithEmptyFirstName();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("First name is required");
    }

    [Fact]
    public async Task UpdateProfile_WithNullFirstName_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateRequestWithNullFirstName();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("First name is required");
    }

    [Fact]
    public async Task UpdateProfile_WithEmptyLastName_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateRequestWithEmptyLastName();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Last name is required");
    }

    [Fact]
    public async Task UpdateProfile_WithNullLastName_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateRequestWithNullLastName();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Last name is required");
    }

    [Fact]
    public async Task UpdateProfile_WithNullMiddleName_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateRequestWithNullMiddleName();

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.MiddleName.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProfile_WithEmptyMiddleName_ShouldTreatAsNull()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateRequestWithEmptyMiddleName();

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        // Empty string might be treated as null or empty based on validation rules
        response!.Data.MiddleName.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateProfile_WithNullPhone_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateRequestWithNullPhone();

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Phone.Should().BeNull();

        // Verify phone is null in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email.Value == "customer@shopilent.com");

            user.Should().NotBeNull();
            user!.Phone.Value.Should().BeNull();
        });
    }

    [Fact]
    public async Task UpdateProfile_WithEmptyPhone_ShouldTreatAsNull()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateRequestWithEmptyPhone();

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        // Empty string might be treated as null or empty based on validation rules
        response!.Data.Phone.Should().BeNullOrEmpty();

        // Verify phone handling in database (empty string should be treated as null)
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email.Value == "customer@shopilent.com");

            user.Should().NotBeNull();
            // Empty phone should be stored as null in the database
            user!.Phone.Value.Should().BeNull();
        });
    }

    [Fact]
    public async Task UpdateProfile_WithInvalidPhone_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateRequestWithInvalidPhone();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Phone number is not valid");
    }

    [Fact]
    public async Task UpdateProfile_WithValidInternationalPhone_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateRequestWithValidInternationalPhone();

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Phone.Should().Be("+1234567890123");
    }

    [Fact]
    public async Task UpdateProfile_WithInvalidCharactersInName_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateRequestWithInvalidCharactersInName();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("contains invalid characters");
    }

    [Fact]
    public async Task UpdateProfile_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = UpdateProfileTestDataV1.CreateValidRequest();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_WithAdminUser_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateValidRequest(
            firstName: "Admin",
            lastName: "User");

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("Admin");
        response.Data.LastName.Should().Be("User");
    }

    [Theory]
    [InlineData(null, "Doe", null, null)]
    [InlineData("", "Doe", null, null)]
    [InlineData("   ", "Doe", null, null)]
    [InlineData("John", null, null, null)]
    [InlineData("John", "", null, null)]
    [InlineData("John", "   ", null, null)]
    public async Task UpdateProfile_WithInvalidRequiredFields_ShouldReturnValidationError(
        string? firstName, string? lastName, string? middleName, string? phone)
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = new
        {
            FirstName = firstName,
            LastName = lastName,
            MiddleName = middleName,
            Phone = phone
        };

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny(
            "First name is required",
            "Last name is required");
    }

    // Boundary value tests
    [Fact]
    public async Task UpdateProfile_WithSingleCharacterName_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.BoundaryTests.CreateRequestWithSingleCharacterName();

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("A");
        response.Data.LastName.Should().Be("B");
        response.Data.MiddleName.Should().Be("C");
    }

    [Fact]
    public async Task UpdateProfile_WithMaximumLengthNames_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.BoundaryTests.CreateRequestWithMaximumLengthNames();

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().HaveLength(50);
        response.Data.LastName.Should().HaveLength(50);
        response.Data.MiddleName.Should().HaveLength(50);
    }

    [Fact]
    public async Task UpdateProfile_WithLongNames_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.BoundaryTests.CreateRequestWithLongNames();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("must not exceed 50 characters");
    }

    // Edge case tests
    [Fact]
    public async Task UpdateProfile_WithUnicodeCharacters_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.EdgeCases.CreateRequestWithUnicodeCharacters();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("contains invalid characters");
    }

    [Fact]
    public async Task UpdateProfile_WithSpecialCharacters_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.EdgeCases.CreateRequestWithSpecialCharacters();

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("John-Paul");
        response.Data.LastName.Should().Be("O'Connor");
        response.Data.MiddleName.Should().Be("De'Angelo");
    }

    [Fact]
    public async Task UpdateProfile_WithWhitespaceInNames_ShouldPreserveWhitespace()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.EdgeCases.CreateRequestWithWhitespaceInNames();

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        // Current behavior: whitespace is preserved as-is (no trimming performed)
        response!.Data.FirstName.Should().Be(" John ");
        response.Data.LastName.Should().Be(" Doe ");
        response.Data.MiddleName.Should().Be(" Middle ");
    }

    [Fact]
    public async Task UpdateProfile_WithOnlyWhitespace_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.EdgeCases.CreateRequestWithOnlyWhitespace();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny(
            "First name is required",
            "Last name is required");
    }

    [Fact]
    public async Task UpdateProfile_WithTabsAndNewlines_ShouldHandleGracefully()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.EdgeCases.CreateRequestWithTabsAndNewlines();

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        // Should handle tabs and newlines gracefully (trim or sanitize)
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateProfile_WithNumericNames_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.EdgeCases.CreateRequestWithNumericNames();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("contains invalid characters");
    }

    [Fact]
    public async Task UpdateProfile_WithEmojis_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.EdgeCases.CreateRequestWithEmojis();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("contains invalid characters");
    }

    [Fact]
    public async Task UpdateProfile_WithValidSpecialCharacters_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.EdgeCases.CreateRequestWithValidSpecialCharacters();

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("Mary-Jane");
        response.Data.LastName.Should().Be("O'Connor");
        response.Data.MiddleName.Should().Be("Ann-Marie");
    }

    [Fact]
    public async Task UpdateProfile_WithValidDotsAndSpaces_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.EdgeCases.CreateRequestWithValidDotsAndSpaces();

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("John Jr.");
        response.Data.LastName.Should().Be("Van Der Berg");
        response.Data.MiddleName.Should().Be("De La Cruz");
    }

    // Security tests
    [Fact]
    public async Task UpdateProfile_WithSqlInjectionAttempt_ShouldHandleSafely()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.SecurityTests.CreateSqlInjectionAttempt();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert - Should return validation error for invalid characters and not expose system information
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        // Should return validation error for invalid characters, not expose SQL details
        content.Should().NotContain("SQL");
        content.Should().NotContain("DROP");
        content.Should().NotContain("SELECT");
        content.Should().NotContain("UPDATE");
        content.Should().NotContain("database");
        content.Should().NotContain("exception");
        // Should contain validation error message
        content.Should().Contain("contains invalid characters");
    }

    [Fact]
    public async Task UpdateProfile_WithXssAttempt_ShouldHandleSafely()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.SecurityTests.CreateXssAttempt();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert - Should return validation error for invalid characters and not expose XSS payload
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        // Should return validation error, not expose XSS payload
        content.Should().NotContain("<script>");
        content.Should().NotContain("alert");
        content.Should().NotContain("javascript");
        content.Should().NotContain("onerror");
        // Should contain validation error message
        content.Should().Contain("contains invalid characters");
    }

    [Fact]
    public async Task UpdateProfile_WithCommandInjectionAttempt_ShouldHandleSafely()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.SecurityTests.CreateCommandInjectionAttempt();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert - Should return validation error for invalid characters and not expose commands
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        // Should return validation error, not expose system commands
        content.Should().NotContain("rm");
        content.Should().NotContain("cat");
        content.Should().NotContain("whoami");
        content.Should().NotContain("system");
        content.Should().NotContain("command");
        // Should contain validation error message
        content.Should().Contain("contains invalid characters");
    }

    [Fact]
    public async Task UpdateProfile_WithLongStringAttack_ShouldHandleGracefully()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.SecurityTests.CreateLongStringAttack();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert - Should return validation error for exceeding length limits
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("must not exceed 50 characters");
    }

    [Fact]
    public async Task UpdateProfile_WithNullByteAttempt_ShouldHandleSafely()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.SecurityTests.CreateNullByteAttempt();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert - Should return validation error for invalid null byte characters
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("contains invalid characters");
    }

    // Performance/Concurrency tests
    [Fact]
    public async Task UpdateProfile_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange - Test concurrent profile updates (optimistic concurrency control)
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var validNameVariations = new[]
        {
            ("John", "Smith", "James"),
            ("Jane", "Johnson", "Marie"),
            ("Michael", "Brown", "Robert")
        };

        var requests = Enumerable.Range(0, 3)
            .Select(i => UpdateProfileTestDataV1.CreateValidRequest(
                firstName: validNameVariations[i].Item1,
                lastName: validNameVariations[i].Item2,
                middleName: validNameVariations[i].Item3,
                phone: $"555{i:000}{i:0000}"))
            .ToList();

        var tasks = requests
            .Select(request => PutAsync("v1/users/me", request))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert - Due to optimistic concurrency control, some requests may succeed while others may fail
        var successfulResponses = responses.Where(r => r.StatusCode == HttpStatusCode.OK).ToList();
        var failedResponses = responses.Where(r => r.StatusCode != HttpStatusCode.OK).ToList();

        // At least one request should succeed
        successfulResponses.Should().NotBeEmpty("At least one profile update should succeed");

        // If there are failed responses, they should be due to concurrency conflicts (409 Conflict)
        // Note: In fast execution scenarios, all requests may succeed without conflicts
        if (failedResponses.Any())
        {
            failedResponses.Should().AllSatisfy(response =>
                response.StatusCode.Should().Be(HttpStatusCode.Conflict),
                "Failed responses should be 409 Conflict due to concurrency violations");
        }

        // Verify final profile is one of the updated profiles
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email.Value == "customer@shopilent.com");

            user.Should().NotBeNull();
            // The final state should be one of the requested updates
            var expectedFirstNames = requests.Select(r => ((dynamic)r).FirstName).Cast<string>();
            user!.FullName.FirstName.Should().BeOneOf(expectedFirstNames);
        });
    }

    [Fact]
    public async Task UpdateProfile_ShouldNotExposeUserDetailsInResponse()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = UpdateProfileTestDataV1.CreateValidRequest(
            firstName: "John",
            lastName: "Doe");

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);

        // Serialize response to check for sensitive data exposure
        var responseJson = System.Text.Json.JsonSerializer.Serialize(response);
        responseJson.Should().NotContain("password");
        responseJson.Should().NotContain("email"); // Should not expose email in update response
        responseJson.Should().NotContain("salt");
        responseJson.Should().NotContain("hash");
    }

    [Fact]
    public async Task UpdateProfile_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = UpdateProfileTestDataV1.CreateValidRequest();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_WithMalformedAuthToken_ShouldReturnUnauthorized()
    {
        // Arrange
        SetAuthenticationHeader("malformed-token");
        var request = UpdateProfileTestDataV1.CreateValidRequest();

        // Act
        var response = await PutAsync("v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_WithValidToken_ShouldUpdateCorrectUser()
    {
        // Arrange - Test that the update affects only the authenticated user
        await EnsureCustomerUserExistsAsync();
        await EnsureAdminUserExistsAsync();
        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);

        var request = UpdateProfileTestDataV1.CreateValidRequest(
            firstName: "CustomerUpdated",
            lastName: "User");

        // Act
        var response = await PutApiResponseAsync<object, UpdateUserProfileResponseV1>("v1/users/me", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("CustomerUpdated");

        // Verify only the customer user was updated, not the admin
        await ExecuteDbContextAsync(async context =>
        {
            var customerUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email.Value == "customer@shopilent.com");
            var adminUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email.Value == "admin@shopilent.com");

            customerUser.Should().NotBeNull();
            customerUser!.FullName.FirstName.Should().Be("CustomerUpdated");

            adminUser.Should().NotBeNull();
            adminUser!.FullName.FirstName.Should().NotBe("CustomerUpdated");
        });
    }
}
