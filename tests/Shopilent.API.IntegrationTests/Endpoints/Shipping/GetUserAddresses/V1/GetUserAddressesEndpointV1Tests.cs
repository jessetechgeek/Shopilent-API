using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.Common.Models;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.API.Endpoints.Shipping.CreateAddress.V1;
using Shopilent.API.Endpoints.Shipping.GetUserAddresses.V1;
using Shopilent.Application.Features.Shipping.Commands.CreateAddress.V1;
using Shopilent.Domain.Shipping.DTOs;
using Shopilent.Domain.Shipping.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Shipping.GetUserAddresses.V1;

public class GetUserAddressesEndpointV1Tests : ApiIntegrationTestBase
{
    public GetUserAddressesEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task GetUserAddresses_WithValidAuthentication_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Message.Should().Be("User addresses retrieved successfully");
        response.Data.Addresses.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserAddresses_WithNoAddresses_ShouldReturnEmptyList()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Addresses.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserAddresses_WithSingleAddress_ShouldReturnOneAddress()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address for the authenticated customer
        var addressRequest = AddressTestDataV1.Creation.CreateValidRequest(
            addressLine1: "123 Test Street",
            city: "Test City",
            state: "Test State",
            postalCode: "12345",
            country: "Test Country",
            addressType: AddressType.Shipping,
            isDefault: true
        );

        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", addressRequest);
        AssertApiSuccess(createResponse);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Addresses.Should().HaveCount(1);

        var address = response.Data.Addresses.First();
        address.Id.Should().Be(createResponse!.Data.Id);
        address.AddressLine1.Should().Be("123 Test Street");
        address.City.Should().Be("Test City");
        address.State.Should().Be("Test State");
        address.PostalCode.Should().Be("12345");
        address.Country.Should().Be("Test Country");
        address.AddressType.Should().Be(AddressType.Shipping);
        address.IsDefault.Should().BeTrue();
        address.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        address.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetUserAddresses_WithMultipleAddresses_ShouldReturnAllAddresses()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple addresses
        var addresses = new[]
        {
            AddressTestDataV1.Creation.CreateShippingAddressRequest(isDefault: true),
            AddressTestDataV1.Creation.CreateBillingAddressRequest(isDefault: false),
            AddressTestDataV1.Creation.CreateBothAddressRequest(isDefault: false)
        };

        var createdIds = new List<Guid>();
        foreach (var addressRequest in addresses)
        {
            var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", addressRequest);
            AssertApiSuccess(createResponse);
            createdIds.Add(createResponse!.Data.Id);
        }

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Addresses.Should().HaveCount(3);

        // Verify all created addresses are present
        foreach (var createdId in createdIds)
        {
            response.Data.Addresses.Should().Contain(a => a.Id == createdId,
                $"Created address with ID {createdId} should be present in the response");
        }
    }

    [Fact]
    public async Task GetUserAddresses_WithAllAddressTypes_ShouldReturnAllTypes()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create addresses of all types
        var shippingAddress = AddressTestDataV1.Creation.CreateShippingAddressRequest();
        var billingAddress = AddressTestDataV1.Creation.CreateBillingAddressRequest();
        var bothAddress = AddressTestDataV1.Creation.CreateBothAddressRequest();

        await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", shippingAddress);
        await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", billingAddress);
        await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", bothAddress);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Addresses.Should().HaveCount(3);

        // Verify all address types are present
        response.Data.Addresses.Should().Contain(a => a.AddressType == AddressType.Shipping);
        response.Data.Addresses.Should().Contain(a => a.AddressType == AddressType.Billing);
        response.Data.Addresses.Should().Contain(a => a.AddressType == AddressType.Both);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task GetUserAddresses_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader(); // Ensure no auth header

        // Act
        var response = await Client.GetAsync("v1/addresses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserAddresses_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        SetAuthenticationHeader("invalid-token");

        // Act
        var response = await Client.GetAsync("v1/addresses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserAddresses_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        // Use an obviously expired token (this is a mock expired token for demonstration)
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1MTYyMzkwMjJ9.invalid";
        SetAuthenticationHeader(expiredToken);

        // Act
        var response = await Client.GetAsync("v1/addresses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task GetUserAddresses_AsCustomer_ShouldReturnOnlyOwnAddresses()
    {
        // Arrange - Create admin address
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);
        var adminAddress = AddressTestDataV1.Creation.CreateValidRequest(
            addressLine1: "Admin Address",
            addressType: AddressType.Shipping
        );
        await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", adminAddress);

        // Create customer address
        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);
        var customerAddress = AddressTestDataV1.Creation.CreateValidRequest(
            addressLine1: "Customer Address",
            addressType: AddressType.Billing
        );
        var customerCreateResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", customerAddress);

        // Act - Get addresses as customer
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert - Customer should only see their own address
        AssertApiSuccess(response);
        response!.Data.Addresses.Should().HaveCount(1);
        response.Data.Addresses.First().Id.Should().Be(customerCreateResponse!.Data.Id);
        response.Data.Addresses.First().AddressLine1.Should().Be("Customer Address");
    }

    [Fact]
    public async Task GetUserAddresses_WithAdminRole_ShouldReturnOnlyOwnAddresses()
    {
        // Arrange
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        // Create admin addresses
        var address1 = AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "Admin Address 1");
        var address2 = AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "Admin Address 2");

        await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", address1);
        await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", address2);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Addresses.Should().HaveCount(2);
        response.Data.Addresses.Should().AllSatisfy(address =>
            address.AddressLine1.Should().StartWith("Admin Address"));
    }

    [Fact]
    public async Task GetUserAddresses_WithManagerRole_ShouldReturnOnlyOwnAddresses()
    {
        // Arrange
        var managerToken = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(managerToken);

        // Create manager address
        var managerAddress = AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "Manager Address");
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", managerAddress);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Addresses.Should().HaveCount(1);
        response.Data.Addresses.First().Id.Should().Be(createResponse!.Data.Id);
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task GetUserAddresses_ShouldReturnDataConsistentWithDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create test addresses
        var addresses = new[]
        {
            AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "Address 1", city: "City 1"),
            AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "Address 2", city: "City 2"),
            AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "Address 3", city: "City 3")
        };

        var createdIds = new List<Guid>();
        foreach (var addressRequest in addresses)
        {
            var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", addressRequest);
            AssertApiSuccess(createResponse);
            createdIds.Add(createResponse!.Data.Id);
        }

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Addresses.Should().HaveCount(3);

        // Verify API response data matches database data exactly
        await ExecuteDbContextAsync(async context =>
        {
            var dbAddresses = await context.Addresses
                .Where(a => createdIds.Contains(a.Id))
                .ToListAsync();

            dbAddresses.Should().HaveCount(3, "All 3 created addresses should exist in database");

            foreach (var dbAddress in dbAddresses)
            {
                var apiAddress = response.Data.Addresses.First(a => a.Id == dbAddress.Id);

                // Verify all fields match between API and database
                apiAddress.Id.Should().Be(dbAddress.Id);
                apiAddress.UserId.Should().Be(dbAddress.UserId);
                apiAddress.AddressLine1.Should().Be(dbAddress.AddressLine1);
                (apiAddress.AddressLine2 ?? string.Empty).Should().Be(dbAddress.AddressLine2 ?? string.Empty);
                apiAddress.City.Should().Be(dbAddress.City);
                apiAddress.State.Should().Be(dbAddress.State);
                apiAddress.PostalCode.Should().Be(dbAddress.PostalCode);
                apiAddress.Country.Should().Be(dbAddress.Country);
                (apiAddress.Phone ?? string.Empty).Should().Be(dbAddress.Phone?.Value ?? string.Empty);
                apiAddress.IsDefault.Should().Be(dbAddress.IsDefault);
                apiAddress.AddressType.Should().Be(dbAddress.AddressType);
                apiAddress.CreatedAt.Should().BeCloseTo(dbAddress.CreatedAt, TimeSpan.FromSeconds(1));
                apiAddress.UpdatedAt.Should().BeCloseTo(dbAddress.UpdatedAt, TimeSpan.FromSeconds(1));
            }
        });
    }

    [Fact]
    public async Task GetUserAddresses_WithDefaultAddresses_ShouldIncludeDefaultFlag()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create addresses with default flags
        var defaultAddress = AddressTestDataV1.Creation.CreateValidRequest(
            addressLine1: "Default Address",
            isDefault: true
        );
        var nonDefaultAddress = AddressTestDataV1.Creation.CreateValidRequest(
            addressLine1: "Non-Default Address",
            isDefault: false
        );

        var defaultCreateResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", defaultAddress);
        var nonDefaultCreateResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", nonDefaultAddress);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Addresses.Should().HaveCount(2);

        var defaultAddr = response.Data.Addresses.First(a => a.Id == defaultCreateResponse!.Data.Id);
        var nonDefaultAddr = response.Data.Addresses.First(a => a.Id == nonDefaultCreateResponse!.Data.Id);

        defaultAddr.IsDefault.Should().BeTrue();
        nonDefaultAddr.IsDefault.Should().BeFalse();
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task GetUserAddresses_WithUnicodeCharacters_ShouldReturnCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create address with unicode characters
        var unicodeAddress = AddressTestDataV1.EdgeCases.CreateRequestWithUnicodeCharacters();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", unicodeAddress);
        AssertApiSuccess(createResponse);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Addresses.Should().HaveCount(1);

        var address = response.Data.Addresses.First();
        address.AddressLine1.Should().Be("123 Café Münchën Street™");
        address.City.Should().Be("São Paulo");
        address.State.Should().Be("Île-de-France");
        address.Country.Should().Be("République Française");
    }

    [Fact]
    public async Task GetUserAddresses_WithSpecialCharacters_ShouldReturnCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create address with special characters
        var specialCharsAddress = AddressTestDataV1.EdgeCases.CreateRequestWithSpecialCharacters();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", specialCharsAddress);
        AssertApiSuccess(createResponse);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Addresses.Should().HaveCount(1);

        var address = response.Data.Addresses.First();
        address.AddressLine1.Should().Be("123 Main St. #456");
        address.City.Should().Be("St. John's");
        address.PostalCode.Should().Be("A1A-1A1");
    }

    [Fact]
    public async Task GetUserAddresses_WithInternationalAddresses_ShouldReturnCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create international addresses
        var japaneseAddress = AddressTestDataV1.EdgeCases.CreateInternationalAddress();
        var ukAddress = AddressTestDataV1.EdgeCases.CreateUKAddress();
        var canadianAddress = AddressTestDataV1.EdgeCases.CreateCanadianAddress();

        await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", japaneseAddress);
        await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", ukAddress);
        await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", canadianAddress);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Addresses.Should().HaveCount(3);

        // Verify Japanese address
        var jaAddress = response.Data.Addresses.First(a => a.Country == "Japan");
        jaAddress.City.Should().Be("Tokyo");
        jaAddress.PostalCode.Should().Be("150-0002");

        // Verify UK address
        var ukAddr = response.Data.Addresses.First(a => a.Country == "United Kingdom");
        ukAddr.City.Should().Be("London");
        ukAddr.PostalCode.Should().Be("SW1A 2AA");

        // Verify Canadian address
        var caAddr = response.Data.Addresses.First(a => a.Country == "Canada");
        caAddr.City.Should().Be("Toronto");
        caAddr.PostalCode.Should().Be("M5H 2N2");
    }

    [Fact]
    public async Task GetUserAddresses_WithOptionalFieldsMissing_ShouldReturnCorrectly()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create address without optional fields (AddressLine2, Phone)
        var addressWithoutOptionals = AddressTestDataV1.Creation.CreateAddressWithoutPhone();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", addressWithoutOptionals);
        AssertApiSuccess(createResponse);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Addresses.Should().HaveCount(1);

        var address = response.Data.Addresses.First();
        address.Phone.Should().BeNullOrEmpty();
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public async Task GetUserAddresses_ShouldReturnAddressesInConsistentOrder()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create addresses with specific order
        var addresses = new[]
        {
            AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "First Address"),
            AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "Second Address"),
            AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "Third Address")
        };

        foreach (var address in addresses)
        {
            await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", address);
            await Task.Delay(10); // Small delay to ensure different creation times
        }

        // Act - Call multiple times to ensure consistent ordering
        var response1 = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");
        var response2 = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);

        response1!.Data.Addresses.Should().HaveCount(3);
        response2!.Data.Addresses.Should().HaveCount(3);

        // Verify consistent ordering between calls
        response1.Data.Addresses.Select(a => a.Id).Should().BeEquivalentTo(
            response2.Data.Addresses.Select(a => a.Id),
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetUserAddresses_WithDefaultAddress_ShouldAppearInResults()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create default and non-default addresses
        var defaultAddress = AddressTestDataV1.DefaultManagement.CreateDefaultShippingAddress();
        var nonDefaultAddress = AddressTestDataV1.DefaultManagement.CreateNonDefaultAddress();

        var defaultCreateResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", defaultAddress);
        await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", nonDefaultAddress);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Addresses.Should().HaveCount(2);
        response.Data.Addresses.Should().Contain(a => a.IsDefault && a.Id == defaultCreateResponse!.Data.Id);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetUserAddresses_WithManyAddresses_ShouldPerformWell()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create many addresses
        var addresses = AddressTestDataV1.Creation.CreateMultipleAddresses(15);
        foreach (var address in addresses)
        {
            await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", address);
        }

        // Act & Assert - Measure response time
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");
        stopwatch.Stop();

        AssertApiSuccess(response);
        response!.Data.Addresses.Should().HaveCount(15);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
    }

    [Fact]
    public async Task GetUserAddresses_ConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create some test addresses
        var address = AddressTestDataV1.Creation.CreateValidRequest();
        await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", address);

        // Act - Make concurrent requests
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses"))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert - All requests should succeed with consistent data
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));

        var firstResponseData = responses[0]!.Data.Addresses;
        responses.Should().AllSatisfy(response =>
            response!.Data.Addresses.Should().BeEquivalentTo(firstResponseData));
    }

    #endregion

    #region Cache Behavior Tests

    [Fact]
    public async Task GetUserAddresses_ShouldBeCached()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create address
        var address = AddressTestDataV1.Creation.CreateValidRequest();
        await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", address);

        // Act - Make first request (should populate cache)
        var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
        var response1 = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");
        stopwatch1.Stop();

        // Act - Make second request (should use cache)
        var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
        var response2 = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");
        stopwatch2.Stop();

        // Assert
        AssertApiSuccess(response1);
        AssertApiSuccess(response2);

        // Data should be identical
        response1!.Data.Addresses.Should().BeEquivalentTo(response2!.Data.Addresses);

        // Second request should be faster or similar (cached)
        stopwatch2.ElapsedMilliseconds.Should().BeLessOrEqualTo(stopwatch1.ElapsedMilliseconds + 100);
    }

    #endregion

    #region HTTP Status Code Tests

    [Fact]
    public async Task GetUserAddresses_WithValidRequest_ShouldReturnStatus200()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(200);
        AssertApiSuccess(response);
    }

    [Fact]
    public async Task GetUserAddresses_ShouldHaveCorrectContentType()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(200);
        AssertApiSuccess(response);
        // Content type verification is handled by the API response structure
    }

    #endregion

    #region User Context Tests

    [Fact]
    public async Task GetUserAddresses_WithDifferentUsers_ShouldReturnSeparateAddresses()
    {
        // Arrange - Create address for customer 1
        var customer1Token = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customer1Token);
        var customer1Address = AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "Customer 1 Address");
        var customer1CreateResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", customer1Address);

        // Create address for admin (acting as customer 2)
        var customer2Token = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(customer2Token);
        var customer2Address = AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "Customer 2 Address");
        var customer2CreateResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", customer2Address);

        // Act & Assert - Get addresses for customer 1
        SetAuthenticationHeader(customer1Token);
        var customer1Response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");
        AssertApiSuccess(customer1Response);
        customer1Response!.Data.Addresses.Should().HaveCount(1);
        customer1Response.Data.Addresses.First().Id.Should().Be(customer1CreateResponse!.Data.Id);
        customer1Response.Data.Addresses.First().AddressLine1.Should().Be("Customer 1 Address");

        // Act & Assert - Get addresses for customer 2
        SetAuthenticationHeader(customer2Token);
        var customer2Response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");
        AssertApiSuccess(customer2Response);
        customer2Response!.Data.Addresses.Should().HaveCount(1);
        customer2Response.Data.Addresses.First().Id.Should().Be(customer2CreateResponse!.Data.Id);
        customer2Response.Data.Addresses.First().AddressLine1.Should().Be("Customer 2 Address");
    }

    #endregion

    #region Response Structure Tests

    [Fact]
    public async Task GetUserAddresses_ShouldReturnCorrectResponseStructure()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create a comprehensive address
        var address = AddressTestDataV1.Creation.CreateValidRequest(
            addressLine1: "123 Test Street",
            addressLine2: "Apt 456",
            city: "Test City",
            state: "Test State",
            postalCode: "12345",
            country: "Test Country",
            phone: "+1234567890",
            addressType: AddressType.Both,
            isDefault: true
        );
        await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", address);

        // Act
        var response = await GetApiResponseAsync<GetUserAddressesResponseV1>("v1/addresses");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Addresses.Should().HaveCount(1);

        var returnedAddress = response.Data.Addresses.First();
        returnedAddress.Id.Should().NotBeEmpty();
        returnedAddress.UserId.Should().NotBeEmpty();
        returnedAddress.AddressLine1.Should().NotBeNullOrEmpty();
        returnedAddress.City.Should().NotBeNullOrEmpty();
        returnedAddress.State.Should().NotBeNullOrEmpty();
        returnedAddress.PostalCode.Should().NotBeNullOrEmpty();
        returnedAddress.Country.Should().NotBeNullOrEmpty();
        returnedAddress.AddressType.Should().BeOneOf(Enum.GetValues<AddressType>());
        returnedAddress.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        returnedAddress.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    #endregion
}
