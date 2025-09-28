using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Catalog.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Attributes.GetAttribute.V1;

public class GetAttributeEndpointV1Tests : ApiIntegrationTestBase
{
    public GetAttributeEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task GetAttribute_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a test attribute first
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: "test_get_attribute",
            displayName: "Test Get Attribute",
            type: "Text");
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);
        var attributeId = createResponse!.Data.Id;

        // Clear auth header to test anonymous access
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<AttributeDto>($"v1/attributes/{attributeId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(attributeId);
        response.Data.Name.Should().Be("test_get_attribute");
        response.Data.DisplayName.Should().Be("Test Get Attribute");
        response.Data.Type.Should().Be(AttributeType.Text);
        response.Data.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        response.Data.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetAttribute_WithValidId_ShouldReturnCorrectData()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a test attribute with specific configuration
        var createRequest = AttributeTestDataV1.TypeSpecific.CreateSelectAttributeRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);
        var attributeId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<AttributeDto>($"v1/attributes/{attributeId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(attributeId);
        response.Data.Name.Should().Be("select_attribute");
        response.Data.DisplayName.Should().Be("Select Attribute");
        response.Data.Type.Should().Be(AttributeType.Select);
        response.Data.Filterable.Should().BeTrue();
        response.Data.Searchable.Should().BeFalse();
        response.Data.IsVariant.Should().BeTrue();
        response.Data.Configuration.Should().NotBeNull();
        response.Data.Configuration.Should().ContainKey("options");
    }

    [Theory]
    [InlineData("Text")]
    [InlineData("Number")]
    [InlineData("Boolean")]
    [InlineData("Select")]
    [InlineData("Color")]
    public async Task GetAttribute_WithDifferentTypes_ShouldReturnCorrectType(string attributeType)
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: $"test_{attributeType.ToLower()}_get",
            type: attributeType);
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);
        var attributeId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<AttributeDto>($"v1/attributes/{attributeId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Type.ToString().Should().Be(attributeType);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetAttribute_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"v1/attributes/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain(nonExistentId.ToString());
        content.Should().ContainAny("not found", "NotFound");
    }

    [Fact]
    public async Task GetAttribute_WithInvalidGuidFormat_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync("v1/attributes/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAttribute_WithEmptyGuid_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync($"v1/attributes/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Anonymous Access Tests

    [Fact]
    public async Task GetAttribute_WithoutAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a test attribute
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);
        var attributeId = createResponse!.Data.Id;

        // Clear authentication
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<AttributeDto>($"v1/attributes/{attributeId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(attributeId);
    }

    [Fact]
    public async Task GetAttribute_WithCustomerRole_ShouldReturnSuccess()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        // Create a test attribute as admin
        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);
        var attributeId = createResponse!.Data.Id;

        // Switch to customer authentication
        await EnsureCustomerUserExistsAsync();
        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);

        // Act
        var response = await GetApiResponseAsync<AttributeDto>($"v1/attributes/{attributeId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(attributeId);
    }

    #endregion

    #region Configuration Data Tests

    [Fact]
    public async Task GetAttribute_WithComplexConfiguration_ShouldReturnCompleteData()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AttributeTestDataV1.EdgeCases.CreateRequestWithComplexConfiguration();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);
        var attributeId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<AttributeDto>($"v1/attributes/{attributeId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Configuration.Should().NotBeNull();
        response.Data.Configuration.Should().ContainKey("options");
        response.Data.Configuration.Should().ContainKey("multiple");
        response.Data.Configuration.Should().ContainKey("searchable");
        response.Data.Configuration.Should().ContainKey("metadata");
    }

    [Fact]
    public async Task GetAttribute_WithEmptyConfiguration_ShouldReturnEmptyConfig()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AttributeTestDataV1.EdgeCases.CreateRequestWithEmptyConfiguration();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);
        var attributeId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<AttributeDto>($"v1/attributes/{attributeId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Configuration.Should().NotBeNull();
        response.Data.Configuration.Should().BeEmpty();
    }

    #endregion

    #region Unicode and Special Characters Tests

    [Fact]
    public async Task GetAttribute_WithUnicodeCharacters_ShouldReturnCorrectData()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AttributeTestDataV1.EdgeCases.CreateRequestWithUnicodeCharacters();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);
        var attributeId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<AttributeDto>($"v1/attributes/{attributeId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Name.Should().Be("café_münchën_");
        response.Data.DisplayName.Should().Be("Café Münchën Attribute™");
        response.Data.Configuration.Should().ContainKey("description");
    }

    #endregion

    #region Data Persistence Tests

    [Fact]
    public async Task GetAttribute_ShouldReturnDataFromDatabase()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: "db_persistence_test",
            displayName: "Database Persistence Test",
            type: "Number",
            filterable: true,
            searchable: false,
            isVariant: true);
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);
        var attributeId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<AttributeDto>($"v1/attributes/{attributeId}");

        // Assert
        AssertApiSuccess(response);

        // Verify data matches what was created
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(attributeId);
        response.Data.Name.Should().Be("db_persistence_test");
        response.Data.DisplayName.Should().Be("Database Persistence Test");
        response.Data.Type.Should().Be(AttributeType.Number);
        response.Data.Filterable.Should().BeTrue();
        response.Data.Searchable.Should().BeFalse();
        response.Data.IsVariant.Should().BeTrue();

        // Verify in database directly
        await ExecuteDbContextAsync(async context =>
        {
            var dbAttribute = await context.Attributes
                .FirstOrDefaultAsync(a => a.Id == attributeId);

            dbAttribute.Should().NotBeNull();
            dbAttribute!.Name.Should().Be("db_persistence_test");
            dbAttribute.DisplayName.Should().Be("Database Persistence Test");
            dbAttribute.Type.Should().Be(AttributeType.Number);
            dbAttribute.Filterable.Should().BeTrue();
            dbAttribute.Searchable.Should().BeFalse();
            dbAttribute.IsVariant.Should().BeTrue();
        });
    }

    #endregion

    #region Caching Tests

    [Fact]
    public async Task GetAttribute_CalledTwice_ShouldReturnConsistentData()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: "cache_consistency_test");
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);
        var attributeId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act - Call twice
        var firstResponse = await GetApiResponseAsync<AttributeDto>($"v1/attributes/{attributeId}");
        var secondResponse = await GetApiResponseAsync<AttributeDto>($"v1/attributes/{attributeId}");

        // Assert
        AssertApiSuccess(firstResponse);
        AssertApiSuccess(secondResponse);

        firstResponse!.Data.Should().NotBeNull();
        secondResponse!.Data.Should().NotBeNull();

        // Data should be identical
        firstResponse.Data.Id.Should().Be(secondResponse.Data.Id);
        firstResponse.Data.Name.Should().Be(secondResponse.Data.Name);
        firstResponse.Data.DisplayName.Should().Be(secondResponse.Data.DisplayName);
        firstResponse.Data.Type.Should().Be(secondResponse.Data.Type);
        firstResponse.Data.CreatedAt.Should().Be(secondResponse.Data.CreatedAt);
        firstResponse.Data.UpdatedAt.Should().Be(secondResponse.Data.UpdatedAt);
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public async Task GetAttribute_ShouldReturnProperApiResponseFormat()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AttributeTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", createRequest);
        AssertApiSuccess(createResponse);
        var attributeId = createResponse!.Data.Id;

        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<AttributeDto>($"v1/attributes/{attributeId}");

        // Assert
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Message.Should().NotBeNullOrEmpty();
        response.Errors.Should().BeEmpty();
        response.StatusCode.Should().Be(200);
    }

    #endregion

    #region All Attribute Types Integration Test

    [Fact]
    public async Task GetAttribute_AllSupportedTypes_ShouldReturnCorrectData()
    {
        // Arrange
        await EnsureAdminUserExistsAsync();
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        var testCases = new[]
        {
            ("Text", AttributeTestDataV1.TypeSpecific.CreateTextAttributeRequest()),
            ("Select", AttributeTestDataV1.TypeSpecific.CreateSelectAttributeRequest()),
            ("Color", AttributeTestDataV1.TypeSpecific.CreateColorAttributeRequest()),
            ("Number", AttributeTestDataV1.TypeSpecific.CreateNumberAttributeRequest()),
            ("Boolean", AttributeTestDataV1.TypeSpecific.CreateBooleanAttributeRequest())
        };

        var attributeIds = new List<Guid>();

        // Create all attribute types
        foreach (var (typeName, request) in testCases)
        {
            var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);
            AssertApiSuccess(createResponse);
            attributeIds.Add(createResponse!.Data.Id);
        }

        ClearAuthenticationHeader();

        // Act & Assert - Retrieve and verify each attribute
        for (int i = 0; i < testCases.Length; i++)
        {
            var (typeName, _) = testCases[i];
            var attributeId = attributeIds[i];

            var response = await GetApiResponseAsync<AttributeDto>($"v1/attributes/{attributeId}");
            AssertApiSuccess(response);

            response!.Data.Should().NotBeNull();
            response.Data.Id.Should().Be(attributeId);
            response.Data.Type.ToString().Should().Be(typeName);
            response.Data.Configuration.Should().NotBeNull();
        }
    }

    #endregion

}
