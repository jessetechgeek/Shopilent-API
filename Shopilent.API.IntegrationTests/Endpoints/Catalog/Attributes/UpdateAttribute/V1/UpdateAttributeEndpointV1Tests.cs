using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Common.Models;
using Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;
using Shopilent.Domain.Catalog.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Attributes.UpdateAttribute.V1;

public class UpdateAttributeEndpointV1Tests : ApiIntegrationTestBase
{
    public UpdateAttributeEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task UpdateAttribute_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: "test_update_attribute",
            displayName: "Original Display Name",
            type: "Text");
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.UpdateScenarios.CreateValidUpdateRequest(
            displayName: "Updated Display Name",
            filterable: true,
            searchable: false,
            isVariant: true);

        // Act
        var response = await PutApiResponseAsync<object, UpdateAttributeResponseV1>($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(attributeId);
        response.Data.DisplayName.Should().Be("Updated Display Name");
        response.Data.Filterable.Should().BeTrue();
        response.Data.Searchable.Should().BeFalse();
        response.Data.IsVariant.Should().BeTrue();
        response.Data.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateAttribute_WithValidData_ShouldUpdateAttributeInDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: "test_db_update",
            displayName: "Original DB Name",
            type: "Number");
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.UpdateScenarios.CreateValidUpdateRequest(
            displayName: "Updated DB Name",
            filterable: false,
            searchable: true,
            isVariant: false);

        // Act
        var response = await PutApiResponseAsync<object, UpdateAttributeResponseV1>($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        AssertApiSuccess(response);

        // Verify attribute updated in database
        await ExecuteDbContextAsync(async context =>
        {
            var attribute = await context.Attributes
                .FirstOrDefaultAsync(a => a.Id == attributeId);

            attribute.Should().NotBeNull();
            attribute!.DisplayName.Should().Be("Updated DB Name");
            attribute.Filterable.Should().BeFalse();
            attribute.Searchable.Should().BeTrue();
            attribute.IsVariant.Should().BeFalse();
            attribute.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        });
    }

    [Fact]
    public async Task UpdateAttribute_WithComplexConfiguration_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: "test_complex_config",
            displayName: "Complex Config Test",
            type: "Select");
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.EdgeCases.CreateRequestWithComplexConfiguration();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAttributeResponseV1>($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Configuration.Should().NotBeNull();
        response.Data.Configuration.Should().ContainKey("string_value");
        response.Data.Configuration.Should().ContainKey("number_value");
        response.Data.Configuration.Should().ContainKey("boolean_value");
        response.Data.Configuration.Should().ContainKey("array_value");
        response.Data.Configuration.Should().ContainKey("nested_object");
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateAttribute_WithInvalidDisplayName_ShouldReturnValidationError(string invalidDisplayName)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.UpdateScenarios.CreateValidUpdateRequest(displayName: invalidDisplayName);

        // Act
        var response = await PutAsync($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Display name is required");
    }

    [Fact]
    public async Task UpdateAttribute_WithNullDisplayName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.Validation.CreateRequestWithNullDisplayName();

        // Act
        var response = await PutAsync($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Display name is required");
    }

    [Fact]
    public async Task UpdateAttribute_WithLongDisplayName_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.Validation.CreateRequestWithLongDisplayName();

        // Act
        var response = await PutAsync($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("100 characters");
    }

    [Fact]
    public async Task UpdateAttribute_WithInvalidAttributeId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var invalidId = Guid.NewGuid();
        var updateRequest = AttributeTestDataV1.UpdateScenarios.CreateValidUpdateRequest();

        // Act
        var response = await PutAsync($"v1/attributes/{invalidId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAttribute_WithMalformedGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var malformedId = "not-a-valid-guid";
        var updateRequest = AttributeTestDataV1.UpdateScenarios.CreateValidUpdateRequest();

        // Act
        var response = await PutAsync($"v1/attributes/{malformedId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task UpdateAttribute_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var attributeId = Guid.NewGuid();
        var updateRequest = AttributeTestDataV1.UpdateScenarios.CreateValidUpdateRequest();

        // Act
        var response = await PutAsync($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAttribute_WithCustomerRole_ShouldReturnForbidden()
    {
        // Arrange

        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var attributeId = Guid.NewGuid();
        var updateRequest = AttributeTestDataV1.UpdateScenarios.CreateValidUpdateRequest();

        // Act
        var response = await PutAsync($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateAttribute_WithAdminRole_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.UpdateScenarios.CreateValidUpdateRequest(displayName: "Admin Updated");

        // Act
        var response = await PutApiResponseAsync<object, UpdateAttributeResponseV1>($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.DisplayName.Should().Be("Admin Updated");
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task UpdateAttribute_WithMaximumValidDisplayNameLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.BoundaryTests.CreateRequestWithMaximumDisplayNameLength();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAttributeResponseV1>($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.DisplayName.Should().HaveLength(100);
    }

    [Fact]
    public async Task UpdateAttribute_WithMinimumValidDisplayNameLength_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.BoundaryTests.CreateRequestWithMinimumValidDisplayName();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAttributeResponseV1>($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.DisplayName.Should().Be("A");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task UpdateAttribute_WithUnicodeCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.EdgeCases.CreateRequestWithUnicodeCharacters();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAttributeResponseV1>($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.DisplayName.Should().Be("Café Münchën Attribute™");
        response.Data.Configuration.Should().ContainKey("unicode_value");
        response.Data.Configuration.Should().ContainKey("emoji");
    }

    [Fact]
    public async Task UpdateAttribute_WithSpecialCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.EdgeCases.CreateRequestWithSpecialCharacters();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAttributeResponseV1>($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.DisplayName.Should().Be("Special-Chars_123!@#");
    }

    [Fact]
    public async Task UpdateAttribute_WithEmptyConfiguration_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.EdgeCases.CreateRequestWithEmptyConfiguration();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAttributeResponseV1>($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Configuration.Should().NotBeNull();
        response.Data.Configuration.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAttribute_WithNullConfiguration_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.EdgeCases.CreateRequestWithNullConfiguration();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAttributeResponseV1>($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Configuration.Should().NotBeNull();
    }

    #endregion

    #region Property Combination Tests

    [Fact]
    public async Task UpdateAttribute_WithAllFilterableSearchableVariant_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.UpdateScenarios.PropertyCombinations.CreateRequestAllFilterableSearchableVariant();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAttributeResponseV1>($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.DisplayName.Should().Be("All True Attribute");
        response.Data.Filterable.Should().BeTrue();
        response.Data.Searchable.Should().BeTrue();
        response.Data.IsVariant.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAttribute_WithAllFalseFlags_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;
        var updateRequest = AttributeTestDataV1.UpdateScenarios.PropertyCombinations.CreateRequestAllFalseFlags();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAttributeResponseV1>($"v1/attributes/{attributeId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.DisplayName.Should().Be("All False Attribute");
        response.Data.Filterable.Should().BeFalse();
        response.Data.Searchable.Should().BeFalse();
        response.Data.IsVariant.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(GetPropertyCombinationTestData))]
    public async Task UpdateAttribute_WithVariousPropertyCombinations_ShouldReturnSuccess(
        object requestData, string expectedDisplayName, bool expectedFilterable, bool expectedSearchable, bool expectedIsVariant)
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create an attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);

        var attributeId = createResponse!.Data.Id;

        // Act
        var response = await PutApiResponseAsync<object, UpdateAttributeResponseV1>($"v1/attributes/{attributeId}", requestData);

        // Assert
        AssertApiSuccess(response);
        response!.Data.DisplayName.Should().Be(expectedDisplayName);
        response.Data.Filterable.Should().Be(expectedFilterable);
        response.Data.Searchable.Should().Be(expectedSearchable);
        response.Data.IsVariant.Should().Be(expectedIsVariant);
    }

    public static IEnumerable<object[]> GetPropertyCombinationTestData()
    {
        yield return new object[]
        {
            AttributeTestDataV1.UpdateScenarios.PropertyCombinations.CreateRequestFilterableOnly(),
            "Filterable Only", true, false, false
        };
        yield return new object[]
        {
            AttributeTestDataV1.UpdateScenarios.PropertyCombinations.CreateRequestSearchableOnly(),
            "Searchable Only", false, true, false
        };
        yield return new object[]
        {
            AttributeTestDataV1.UpdateScenarios.PropertyCombinations.CreateRequestVariantOnly(),
            "Variant Only", false, false, true
        };
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task UpdateAttribute_MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple attributes first
        var attributeIds = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var createRequest = AttributeTestDataV1.Creation.CreateValidRequest(
                name: $"concurrent_test_{i}",
                displayName: $"Concurrent Test {i}");
            var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
            AssertApiSuccess(createResponse);
            attributeIds.Add(createResponse!.Data.Id);
        }

        // Create concurrent update tasks
        var tasks = attributeIds.Select(id =>
        {
            var updateRequest = AttributeTestDataV1.UpdateScenarios.CreateValidUpdateRequest(
                displayName: $"Updated Concurrent {id}");
            return PutApiResponseAsync<object, UpdateAttributeResponseV1>($"v1/attributes/{id}", updateRequest);
        }).ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Select(r => r!.Data.Id).Should().OnlyHaveUniqueItems();
        responses.Should().AllSatisfy(response =>
            response!.Data.DisplayName.Should().StartWith("Updated Concurrent"));
    }

    #endregion

    // Response DTO for this specific endpoint version
    public class UpdateAttributeResponseV1
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public AttributeType Type { get; set; }
        public bool Filterable { get; set; }
        public bool Searchable { get; set; }
        public bool IsVariant { get; set; }
        public Dictionary<string, object> Configuration { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
    }

}
