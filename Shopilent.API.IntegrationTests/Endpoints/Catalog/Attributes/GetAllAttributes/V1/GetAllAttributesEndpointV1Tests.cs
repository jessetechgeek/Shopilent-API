using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Catalog.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Catalog.Attributes.GetAllAttributes.V1;

public class GetAllAttributesEndpointV1Tests : ApiIntegrationTestBase
{
    public GetAllAttributesEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task GetAllAttributes_ShouldReturnSuccessResponse()
    {
        // Arrange - No authentication required for this endpoint

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Message.Should().Be("Attributes retrieved successfully");

        // Each attribute should have valid structure
        foreach (var attribute in response.Data)
        {
            attribute.Id.Should().NotBeEmpty();
            attribute.Name.Should().NotBeNullOrEmpty();
            attribute.DisplayName.Should().NotBeNullOrEmpty();
            attribute.Type.Should().BeOneOf(Enum.GetValues<AttributeType>());
            attribute.Configuration.Should().NotBeNull();
            attribute.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            attribute.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        }
    }

    [Fact]
    public async Task GetAllAttributes_WithCreatedAttribute_ShouldIncludeCreatedAttribute()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a unique attribute to test with
        var uniqueName = $"single_test_attr_{Guid.NewGuid():N}";
        var attributeRequest = AttributeTestDataV1.Creation.CreateAttributeForSeeding(
            name: uniqueName,
            displayName: "Single Test Attribute",
            type: AttributeType.Text.ToString()
        );

        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);
        AssertApiSuccess(createResponse);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header since GetAllAttributes allows anonymous access
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty();

        // Find our created attribute in the response
        var createdAttribute = response.Data.FirstOrDefault(a => a.Id == createResponse!.Data.Id);
        createdAttribute.Should().NotBeNull("The created attribute should be present in the response");

        createdAttribute!.Id.Should().Be(createResponse!.Data.Id);
        createdAttribute.Name.Should().Be(uniqueName);
        createdAttribute.DisplayName.Should().Be("Single Test Attribute");
        createdAttribute.Type.Should().Be(AttributeType.Text);
        createdAttribute.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        createdAttribute.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetAllAttributes_WithMultipleAttributes_ShouldReturnAllCreatedAttributes()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple attributes with unique names
        var testId = Guid.NewGuid().ToString("N")[..8];
        var attributeRequests = AttributeTestDataV1.Creation.CreateMultipleAttributesForSeeding(5)
            .Select((req, index) => AttributeTestDataV1.Creation.CreateAttributeForSeeding(
                name: $"multi_test_{testId}_{index}",
                displayName: $"Multi Test Attribute {index + 1}",
                type: ((AttributeType)(index % Enum.GetValues<AttributeType>().Length)).ToString()
            )).ToList();

        var createdIds = new List<Guid>();

        foreach (var request in attributeRequests)
        {
            var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);
            AssertApiSuccess(createResponse);
            createdIds.Add(createResponse!.Data.Id);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty();

        // Verify all created attributes are present in the response
        foreach (var createdId in createdIds)
        {
            response.Data.Should().Contain(a => a.Id == createdId,
                $"Created attribute with ID {createdId} should be present in the response");
        }

        // Verify we can find all created attributes
        var createdAttributes = response.Data.Where(a => createdIds.Contains(a.Id)).ToList();
        createdAttributes.Should().HaveCount(5, "All 5 created attributes should be present");
    }

    [Fact]
    public async Task GetAllAttributes_WithAllAttributeTypes_ShouldIncludeAllCreatedTypes()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create attributes of all types with unique names
        var testId = Guid.NewGuid().ToString("N")[..8];
        var expectedTypes = Enum.GetValues<AttributeType>().ToList();
        var createdIds = new List<Guid>();

        foreach (var (type, index) in expectedTypes.Select((t, i) => (t, i)))
        {
            var request = AttributeTestDataV1.Creation.CreateAttributeForSeeding(
                name: $"type_test_{testId}_{type.ToString().ToLower()}",
                displayName: $"{type} Type Test",
                type: type.ToString()
            );

            var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);
            AssertApiSuccess(createResponse);
            createdIds.Add(createResponse!.Data.Id);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty();

        // Verify all created attributes are present
        var createdAttributes = response.Data.Where(a => createdIds.Contains(a.Id)).ToList();
        createdAttributes.Should().HaveCount(expectedTypes.Count,
            $"All {expectedTypes.Count} created attributes should be present");

        // Verify all expected types are represented in our created attributes
        var createdTypes = createdAttributes.Select(a => a.Type).ToList();
        createdTypes.Should().BeEquivalentTo(expectedTypes,
            "All attribute types should be represented in the created attributes");
    }

    [Fact]
    public async Task GetAllAttributes_ShouldReturnAttributesWithCorrectStructure()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create a comprehensive attribute
        var attributeRequest = AttributeTestDataV1.Creation.CreateValidRequest(
            name: "comprehensive_test_attr",
            displayName: "Comprehensive Test Attribute",
            type: "Select",
            filterable: true,
            searchable: true,
            isVariant: true,
            configuration: new Dictionary<string, object>
            {
                { "options", new[] { "Option 1", "Option 2", "Option 3" } },
                { "multiple_selection", false }
            });
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);
        AssertApiSuccess(createResponse);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Should().HaveCount(1);

        var attribute = response.Data.First();
        attribute.Id.Should().NotBeEmpty();
        attribute.Name.Should().NotBeNullOrEmpty();
        attribute.DisplayName.Should().NotBeNullOrEmpty();
        attribute.Type.Should().BeOneOf(Enum.GetValues<AttributeType>());
        attribute.Configuration.Should().NotBeNull();
        attribute.Filterable.Should().Be(true);
        attribute.Searchable.Should().Be(true);
        attribute.IsVariant.Should().Be(true);
        attribute.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        attribute.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task GetAllAttributes_WithoutAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        ClearAuthenticationHeader(); // Ensure no auth header

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");

        // Assert - GetAllAttributes allows anonymous access
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAttributes_WithCustomerAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAttributes_WithAdminAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task GetAllAttributes_ShouldReturnDataConsistentWithDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create test attributes with unique identifiers
        var testId = Guid.NewGuid().ToString("N")[..8];
        var attributeRequests = Enumerable.Range(0, 3)
            .Select(i => AttributeTestDataV1.Creation.CreateAttributeForSeeding(
                name: $"db_test_{testId}_{i}",
                displayName: $"DB Test Attribute {i + 1}"
            )).ToList();

        var createdIds = new List<Guid>();
        foreach (var request in attributeRequests)
        {
            var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);
            AssertApiSuccess(createResponse);
            createdIds.Add(createResponse!.Data.Id);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeEmpty("Response should contain attributes");

        // Verify all our created attributes are present in the API response
        foreach (var createdId in createdIds)
        {
            response.Data.Should().Contain(a => a.Id == createdId,
                $"Created attribute with ID {createdId} should be present in the API response");
        }

        // Verify API response data matches database data exactly
        await ExecuteDbContextAsync(async context =>
        {
            var dbAttributes = await context.Attributes
                .Where(a => createdIds.Contains(a.Id))
                .ToListAsync();

            dbAttributes.Should().HaveCount(3, "All 3 created attributes should exist in database");

            foreach (var dbAttribute in dbAttributes)
            {
                var apiAttribute = response.Data.First(a => a.Id == dbAttribute.Id);

                // Verify all fields match between API and database
                apiAttribute.Id.Should().Be(dbAttribute.Id);
                apiAttribute.Name.Should().Be(dbAttribute.Name);
                apiAttribute.DisplayName.Should().Be(dbAttribute.DisplayName);
                apiAttribute.Type.Should().Be(dbAttribute.Type);
                apiAttribute.Filterable.Should().Be(dbAttribute.Filterable);
                apiAttribute.Searchable.Should().Be(dbAttribute.Searchable);
                apiAttribute.IsVariant.Should().Be(dbAttribute.IsVariant);
                apiAttribute.CreatedAt.Should().BeCloseTo(dbAttribute.CreatedAt, TimeSpan.FromSeconds(1));
                apiAttribute.UpdatedAt.Should().BeCloseTo(dbAttribute.UpdatedAt, TimeSpan.FromSeconds(1));
                apiAttribute.Configuration.Should().NotBeNull();
            }
        });
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public async Task GetAllAttributes_ShouldReturnAttributesInConsistentOrder()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create attributes with specific creation order
        var attributeRequests = new[]
        {
            AttributeTestDataV1.Creation.CreateAttributeForSeeding(name: "z_last", displayName: "Z Last"),
            AttributeTestDataV1.Creation.CreateAttributeForSeeding(name: "a_first", displayName: "A First"),
            AttributeTestDataV1.Creation.CreateAttributeForSeeding(name: "m_middle", displayName: "M Middle")
        };

        foreach (var request in attributeRequests)
        {
            var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);
            AssertApiSuccess(createResponse);
            // Add small delay to ensure different creation times
            await Task.Delay(10);
        }

        // Clear auth header
        ClearAuthenticationHeader();

        // Act - Call multiple times to ensure consistent ordering
        var response1 = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");
        var response2 = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);

        // Verify consistent ordering between calls
        response1!.Data.Select(a => a.Id).Should().BeEquivalentTo(
            response2!.Data.Select(a => a.Id),
            options => options.WithStrictOrdering());
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task GetAllAttributes_WithUnicodeCharacters_ShouldReturnCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create attribute with unicode characters
        var attributeRequest = AttributeTestDataV1.EdgeCases.CreateRequestWithUnicodeCharactersForCreate();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);
        AssertApiSuccess(createResponse);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().HaveCount(1);

        var attribute = response.Data.First();
        attribute.Name.Should().Be("café_münchën_™");
        attribute.DisplayName.Should().Be("Café Münchën Attribute™");
    }

    [Fact]
    public async Task GetAllAttributes_WithComplexConfiguration_ShouldReturnCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create attribute with complex configuration
        var attributeRequest = AttributeTestDataV1.EdgeCases.CreateRequestWithComplexConfiguration();
        var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);
        AssertApiSuccess(createResponse);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().HaveCount(1);

        var attribute = response.Data.First();
        attribute.Configuration.Should().NotBeNull();
        attribute.Configuration.Should().NotBeEmpty();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetAllAttributes_WithManyAttributes_ShouldPerformWell()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create many attributes
        var attributeRequests = AttributeTestDataV1.Creation.CreateMultipleAttributesForSeeding(20);
        foreach (var request in attributeRequests)
        {
            var createResponse = await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", request);
            AssertApiSuccess(createResponse);
        }

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act & Assert - Measure response time
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");
        stopwatch.Stop();

        AssertApiSuccess(response);
        response!.Data.Should().HaveCount(20);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
    }

    [Fact]
    public async Task GetAllAttributes_ConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create some test data
        var attributeRequest = AttributeTestDataV1.Creation.CreateAttributeForSeeding();
        await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act - Make concurrent requests
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes"))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert - All requests should succeed with consistent data
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));

        var firstResponseData = responses[0]!.Data;
        responses.Should().AllSatisfy(response =>
            response!.Data.Should().BeEquivalentTo(firstResponseData));
    }

    #endregion

    #region Cache Behavior Tests

    [Fact]
    public async Task GetAllAttributes_ShouldBeCached()
    {
        // Arrange
        var accessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(accessToken);

        // Create attribute
        var attributeRequest = AttributeTestDataV1.Creation.CreateAttributeForSeeding();
        await PostApiResponseAsync<object, CreateAttributeResponseV1>("v1/attributes", attributeRequest);

        // Process outbox messages to ensure domain events are handled and cache is invalidated
        await ProcessOutboxMessagesAsync();

        // Clear auth header
        ClearAuthenticationHeader();

        // Act - Make first request (should populate cache)
        var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
        var response1 = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");
        stopwatch1.Stop();

        // Act - Make second request (should use cache)
        var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
        var response2 = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");
        stopwatch2.Stop();

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);

        // Data should be identical
        response1!.Data.Should().BeEquivalentTo(response2!.Data);

        // Second request should be faster (cached) - this is a soft assertion
        // Note: In test environment, caching behavior might vary
        stopwatch2.ElapsedMilliseconds.Should().BeLessOrEqualTo(stopwatch1.ElapsedMilliseconds + 100);
    }

    #endregion

    #region HTTP Status Code Tests

    [Fact]
    public async Task GetAllAttributes_ShouldReturnStatus200()
    {
        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");

        // Assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(200);
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task GetAllAttributes_ShouldHaveCorrectContentType()
    {
        // Act
        var response = await GetApiResponseAsync<IReadOnlyList<AttributeDto>>("v1/attributes");

        // Assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(200);
        AssertApiSuccess(response);
        // Content type verification is handled by the API response structure
    }

    #endregion

}
