using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.Register.V1;

public class RegisterEndpointV1Tests : ApiIntegrationTestBase
{
    public RegisterEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var request = RegisterTestDataV1.CreateValidRequest();

        // Act
        var response = await PostApiResponseAsync<object, RegisterResponseV1>("v1/auth/register", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().NotBeEmpty();
        response.Data.Email.Should().NotBeNullOrEmpty();
        response.Data.FirstName.Should().NotBeNullOrEmpty();
        response.Data.LastName.Should().NotBeNullOrEmpty();
        response.Data.AccessToken.Should().NotBeNullOrEmpty();
        response.Data.RefreshToken.Should().NotBeNullOrEmpty();
        response.Data.EmailVerified.Should().BeFalse(); // New users should not be verified by default
    }

    [Fact]
    public async Task Register_WithValidData_ShouldCreateUserInDatabase()
    {
        // Arrange
        var request = RegisterTestDataV1.CreateValidRequest(
            email: "newuser@example.com",
            firstName: "John",
            lastName: "Doe");

        // Act
        var response = await PostApiResponseAsync<object, RegisterResponseV1>("v1/auth/register", request);

        // Assert
        AssertApiSuccess(response);

        // Verify user exists in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email.Value == "newuser@example.com");

            user.Should().NotBeNull();
            user!.FullName.FirstName.Should().Be("John");
            user.FullName.LastName.Should().Be("Doe");
            user.EmailVerified.Should().BeFalse();
            user.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnValidationError()
    {
        // Arrange
        var request = RegisterTestDataV1.CreateRequestWithInvalidEmail();

        // Act
        var response = await PostAsync("v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Email is not valid.");
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturnValidationError()
    {
        // Arrange
        var request = RegisterTestDataV1.CreateRequestWithWeakPassword();

        // Act
        var response = await PostAsync("v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        // Check for password validation message (weak password = "weak" triggers length requirement)
        content.Should().Contain("Password must be at least 8 characters long.");
    }

    [Fact]
    public async Task Register_WithEmptyFirstName_ShouldReturnValidationError()
    {
        // Arrange
        var request = RegisterTestDataV1.CreateRequestWithEmptyFirstName();

        // Act
        var response = await PostAsync("v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("First name is required.");
    }

    [Fact]
    public async Task Register_WithEmptyLastName_ShouldReturnValidationError()
    {
        // Arrange
        var request = RegisterTestDataV1.CreateRequestWithEmptyLastName();

        // Act
        var response = await PostAsync("v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Last name is required.");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var email = "duplicate@example.com";
        var firstRequest = RegisterTestDataV1.CreateValidRequest(email: email);
        var secondRequest = RegisterTestDataV1.CreateValidRequest(email: email);

        // Act - Register first user
        var firstResponse = await PostApiResponseAsync<object, RegisterResponseV1>("v1/auth/register", firstRequest);
        AssertApiSuccess(firstResponse);

        // Act - Try to register second user with same email
        var secondResponse = await PostAsync("v1/auth/register", secondRequest);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var content = await secondResponse.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        // Check for duplicate email error from domain layer
        content.Should().ContainAny("already exists", "already registered", "duplicate");
    }

    [Fact]
    public async Task Register_WithMissingEmail_ShouldReturnValidationError()
    {
        // Arrange
        var request = RegisterTestDataV1.CreateRequestWithMissingEmail();

        // Act
        var response = await PostAsync("v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithMissingPassword_ShouldReturnValidationError()
    {
        // Arrange
        var request = RegisterTestDataV1.CreateRequestWithMissingPassword();

        // Act
        var response = await PostAsync("v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithLongNames_ShouldReturnValidationError()
    {
        // Arrange
        var request = RegisterTestDataV1.CreateRequestWithLongNames();

        // Act
        var response = await PostAsync("v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("First name must not exceed 100 characters.", "Last name must not exceed 100 characters.");
    }

    [Fact]
    public async Task Register_WithSpecialCharactersInName_ShouldReturnSuccess()
    {
        // Arrange - Some names legitimately contain special characters
        var request = RegisterTestDataV1.CreateRequestWithSpecialCharactersInNames();

        // Act
        var response = await PostApiResponseAsync<object, RegisterResponseV1>("v1/auth/register", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("José");
        response.Data.LastName.Should().Be("O'Connor");
    }

    [Fact]
    public async Task Register_ShouldReturnDifferentTokensForDifferentUsers()
    {
        // Arrange
        var firstRequest = RegisterTestDataV1.CreateValidRequest();
        var secondRequest = RegisterTestDataV1.CreateValidRequest();

        // Act
        var firstResponse = await PostApiResponseAsync<object, RegisterResponseV1>("v1/auth/register", firstRequest);
        var secondResponse = await PostApiResponseAsync<object, RegisterResponseV1>("v1/auth/register", secondRequest);

        // Assert
        AssertApiSuccess(firstResponse);
        AssertApiSuccess(secondResponse);

        firstResponse!.Data.AccessToken.Should().NotBe(secondResponse!.Data.AccessToken);
        firstResponse.Data.RefreshToken.Should().NotBe(secondResponse.Data.RefreshToken);
        firstResponse.Data.Id.Should().NotBe(secondResponse.Data.Id);
    }

    [Fact]
    public async Task Register_ShouldSetDefaultUserRole()
    {
        // Arrange
        var request = RegisterTestDataV1.CreateValidRequest();

        // Act
        var response = await PostApiResponseAsync<object, RegisterResponseV1>("v1/auth/register", request);

        // Assert
        AssertApiSuccess(response);

        // Verify user role in database
        await ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == response!.Data.Id);

            user.Should().NotBeNull();
            user!.Role.Should().Be(Domain.Identity.Enums.UserRole.Customer); // Default role should be Customer
        });
    }

    [Fact]
    public async Task Register_WithMinimumValidPassword_ShouldReturnSuccess()
    {
        // Arrange
        var request = RegisterTestDataV1.PasswordTests.CreateRequestWithMinimumValidPassword();

        // Act
        var response = await PostApiResponseAsync<object, RegisterResponseV1>("v1/auth/register", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithPasswordMissingUppercase_ShouldReturnValidationError()
    {
        // Arrange
        var request = RegisterTestDataV1.PasswordTests.CreateRequestWithPasswordMissingUppercase();

        // Act
        var response = await PostAsync("v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithPasswordMissingDigit_ShouldReturnValidationError()
    {
        // Arrange
        var request = RegisterTestDataV1.PasswordTests.CreateRequestWithPasswordMissingDigit();

        // Act
        var response = await PostAsync("v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithTooShortPassword_ShouldReturnValidationError()
    {
        // Arrange
        var request = RegisterTestDataV1.PasswordTests.CreateRequestWithTooShortPassword();

        // Act
        var response = await PostAsync("v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithExtremelyLongPassword_ShouldReturnValidationError()
    {
        // Arrange
        var request = RegisterTestDataV1.PasswordTests.CreateRequestWithExtremelyLongPassword();

        // Act
        var response = await PostAsync("v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Check for FastEndpoints validation error format
        content.Should().Contain("255 characters");
        content.Should().Contain("password");
    }

    [Fact]
    public async Task Register_WithSingleCharacterNames_ShouldReturnSuccess()
    {
        // Arrange
        var request = RegisterTestDataV1.NameTests.CreateRequestWithSingleCharacterNames();

        // Act
        var response = await PostApiResponseAsync<object, RegisterResponseV1>("v1/auth/register", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("A");
        response.Data.LastName.Should().Be("B");
    }

    [Fact]
    public async Task Register_WithHyphenatedNames_ShouldReturnSuccess()
    {
        // Arrange
        var request = RegisterTestDataV1.NameTests.CreateRequestWithHyphenatedNames();

        // Act
        var response = await PostApiResponseAsync<object, RegisterResponseV1>("v1/auth/register", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("Mary-Jane");
        response.Data.LastName.Should().Be("Smith-Wilson");
    }

    [Fact]
    public async Task Register_WithUnicodeNames_ShouldReturnSuccess()
    {
        // Arrange
        var request = RegisterTestDataV1.NameTests.CreateRequestWithUnicodeNames();

        // Act
        var response = await PostApiResponseAsync<object, RegisterResponseV1>("v1/auth/register", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.FirstName.Should().Be("Björk");
        response.Data.LastName.Should().Be("Müller");
    }

    [Fact]
    public async Task Register_WithExtremelyLongEmail_ShouldReturnValidationError()
    {
        // Arrange
        var request = RegisterTestDataV1.CreateRequestWithExtremelyLongEmail();

        // Act
        var response = await PostAsync("v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("255 characters");
        content.Should().Contain("email");
    }

    [Fact]
    public async Task Register_WithMultipleAtSymbolsInEmail_ShouldReturnValidationError()
    {
        // Arrange
        var request = RegisterTestDataV1.CreateRequestWithMultipleAtSymbolsInEmail();

        // Act
        var response = await PostAsync("v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Register_WithEmptyOrWhitespacePassword_ShouldReturnValidationError(string password)
    {
        // Arrange
        var request = RegisterTestDataV1.CreateRequestWithSpecialPassword(password);

        // Act
        var response = await PostAsync("v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Response DTO for this specific endpoint version
    public class RegisterResponseV1
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
