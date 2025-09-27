using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;
using Shopilent.Domain.Catalog.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Attributes.CreateAttribute.V1;

public class CreateAttributeEndpointV1Tests : ApiIntegrationTestBase
{
    public CreateAttributeEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task CreateAttribute_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateValidRequest();

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().NotBeEmpty();
        response.Data.Name.Should().NotBeNullOrEmpty();
        response.Data.DisplayName.Should().NotBeNullOrEmpty();
        response.Data.Type.Should().BeOneOf(Enum.GetValues<AttributeType>());
        response.Data.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreateAttribute_WithValidData_ShouldCreateAttributeInDatabase()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateValidRequest(
            name: "test_attribute_db",
            displayName: "Test Attribute DB",
            type: "Text");

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);

        // Verify attribute exists in database
        await ExecuteDbContextAsync(async context =>
        {
            var attribute = await context.Attributes
                .FirstOrDefaultAsync(a => a.Id == response!.Data.Id);

            attribute.Should().NotBeNull();
            attribute!.Name.Should().Be("test_attribute_db");
            attribute.DisplayName.Should().Be("Test Attribute DB");
            attribute.Type.Should().Be(AttributeType.Text);
            attribute.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        });
    }

    [Theory]
    [InlineData("Text")]
    [InlineData("Number")]
    [InlineData("Boolean")]
    [InlineData("Select")]
    [InlineData("Color")]
    [InlineData("Date")]
    [InlineData("Dimensions")]
    [InlineData("Weight")]
    public async Task CreateAttribute_WithAllValidTypes_ShouldReturnSuccess(string attributeType)
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateValidRequest(type: attributeType);

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Type.ToString().Should().Be(attributeType);
    }

    #endregion

    #region Type-Specific Tests

    [Fact]
    public async Task CreateAttribute_WithTextType_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.TypeSpecificCases.CreateTextAttributeRequest();

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("text_attribute");
        response.Data.Type.Should().Be(AttributeType.Text);
        response.Data.Configuration.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAttribute_WithSelectType_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.TypeSpecificCases.CreateSelectAttributeRequest();

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("select_attribute");
        response.Data.Type.Should().Be(AttributeType.Select);
        response.Data.IsVariant.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAttribute_WithColorType_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.TypeSpecificCases.CreateColorAttributeRequest();

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Type.Should().Be(AttributeType.Color);
        response.Data.IsVariant.Should().BeTrue();
        response.Data.Filterable.Should().BeTrue();
    }

    #endregion

    #region Validation Tests - Name

    [Fact]
    public async Task CreateAttribute_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateRequestWithEmptyName();

        // Act
        var response = await PostAsync("v1/attributes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Attribute name is required.");
    }

    [Fact]
    public async Task CreateAttribute_WithNullName_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateRequestWithNullName();

        // Act
        var response = await PostAsync("v1/attributes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Attribute name is required.");
    }

    [Fact]
    public async Task CreateAttribute_WithWhitespaceName_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateRequestWithWhitespaceName();

        // Act
        var response = await PostAsync("v1/attributes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Attribute name is required.");
    }

    [Fact]
    public async Task CreateAttribute_WithLongName_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateRequestWithLongName();

        // Act
        var response = await PostAsync("v1/attributes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Attribute name must not exceed 100 characters.");
    }

    #endregion

    #region Validation Tests - DisplayName

    [Fact]
    public async Task CreateAttribute_WithEmptyDisplayName_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateRequestWithEmptyDisplayName();

        // Act
        var response = await PostAsync("v1/attributes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Display name is required.");
    }

    [Fact]
    public async Task CreateAttribute_WithLongDisplayName_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateRequestWithLongDisplayName();

        // Act
        var response = await PostAsync("v1/attributes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Display name must not exceed 100 characters.");
    }

    #endregion

    #region Validation Tests - Type

    [Fact]
    public async Task CreateAttribute_WithEmptyType_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateRequestWithEmptyType();

        // Act
        var response = await PostAsync("v1/attributes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Type is required.");
    }

    [Fact]
    public async Task CreateAttribute_WithInvalidType_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateRequestWithInvalidType();

        // Act
        var response = await PostAsync("v1/attributes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny(
            "Type is invalid. Valid values are: Text, Number, Boolean, Select, Color, Date, Dimensions, Weight.",
            "Invalid attribute type: InvalidType"
        );
    }

    #endregion

    #region Case Sensitivity Tests

    [Theory]
    [InlineData("text")]
    [InlineData("TEXT")]
    [InlineData("Text")]
    [InlineData("tExT")]
    public async Task CreateAttribute_WithDifferentCaseTypes_ShouldReturnSuccess(string type)
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateValidRequest(type: type);

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Type.Should().Be(AttributeType.Text);
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task CreateAttribute_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var request = CreateAttributeTestDataV1.CreateValidRequest();

        // Act
        var response = await PostAsync("v1/attributes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAttribute_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange
        await EnsureCustomerUserExistsAsync();
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateValidRequest();

        // Act
        var response = await PostAsync("v1/attributes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateAttribute_WithAdminRole_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateValidRequest();

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task CreateAttribute_WithMaximumNameLength_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.BoundaryTests.CreateRequestWithMaximumNameLength();

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Length.Should().Be(100);
    }

    [Fact]
    public async Task CreateAttribute_WithMaximumDisplayNameLength_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.BoundaryTests.CreateRequestWithMaximumDisplayNameLength();

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.DisplayName.Length.Should().Be(100);
    }

    [Fact]
    public async Task CreateAttribute_WithMinimumValidName_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.BoundaryTests.CreateRequestWithMinimumValidName();

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("a");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task CreateAttribute_WithUnicodeCharacters_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.EdgeCases.CreateRequestWithUnicodeCharacters();

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Name.Should().Be("café_münchën_™");
        response.Data.DisplayName.Should().Be("Café Münchën Attribute™");
    }

    [Fact]
    public async Task CreateAttribute_WithSpecialCharacters_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.EdgeCases.CreateRequestWithSpecialCharacters();

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.DisplayName.Should().Be("Test Attribute @ 2024!");
    }

    [Fact]
    public async Task CreateAttribute_WithEmptyConfiguration_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.EdgeCases.CreateRequestWithEmptyConfiguration();

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Configuration.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAttribute_WithComplexConfiguration_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.EdgeCases.CreateRequestWithComplexConfiguration();

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Configuration.Should().NotBeNull();
        response.Data.Configuration.Should().ContainKey("options");
    }

    #endregion

    #region Duplicate/Conflict Tests

    [Fact]
    public async Task CreateAttribute_WithDuplicateName_ShouldReturnConflict()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var uniqueName = $"duplicate_test_{Guid.NewGuid():N}";
        var firstRequest = CreateAttributeTestDataV1.CreateValidRequest(name: uniqueName);
        var secondRequest = CreateAttributeTestDataV1.CreateValidRequest(name: uniqueName);

        // Act - Create first attribute
        var firstResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", firstRequest);
        AssertApiSuccess(firstResponse);

        // Act - Try to create second attribute with same name
        var secondResponse = await PostAsync("v1/attributes", secondRequest);

        // Assert
        secondResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);

        var content = await secondResponse.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("already exists", "duplicate", "name");
    }

    #endregion

    #region Bulk/Performance Tests

    [Fact]
    public async Task CreateAttribute_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var tasks = Enumerable.Range(0, 5)
            .Select(i => CreateAttributeTestDataV1.CreateValidRequest(name: $"concurrent_test_{i}_{Guid.NewGuid():N}"))
            .Select(request => PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Select(r => r!.Data.Id).Should().OnlyHaveUniqueItems();
        responses.Select(r => r!.Data.Name).Should().OnlyHaveUniqueItems();
    }

    #endregion

    #region Comprehensive Field Tests

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, false)]
    public async Task CreateAttribute_WithVariousBooleanCombinations_ShouldReturnSuccess(
        bool filterable, bool searchable, bool isVariant)
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);
        var request = CreateAttributeTestDataV1.CreateValidRequest(
            filterable: filterable,
            searchable: searchable,
            isVariant: isVariant);

        // Act
        var response = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Filterable.Should().Be(filterable);
        response.Data.Searchable.Should().Be(searchable);
        response.Data.IsVariant.Should().Be(isVariant);
    }

    #endregion

    // Response DTO for this specific endpoint version
    public class CreateAttributeResponseV1
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public AttributeType Type { get; set; }
        public bool Filterable { get; set; }
        public bool Searchable { get; set; }
        public bool IsVariant { get; set; }
        public Dictionary<string, object> Configuration { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}