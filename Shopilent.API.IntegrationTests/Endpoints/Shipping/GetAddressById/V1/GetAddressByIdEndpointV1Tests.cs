using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Domain.Shipping.DTOs;
using Shopilent.Domain.Shipping.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Shipping.GetAddressById.V1;

public class GetAddressByIdEndpointV1Tests : ApiIntegrationTestBase
{
    public GetAddressByIdEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task GetAddressById_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create a test address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest(
            addressLine1: "123 Test Street",
            city: "New York",
            addressType: AddressType.Shipping);
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(addressId);
        response.Data.AddressLine1.Should().Be("123 Test Street");
        response.Data.City.Should().Be("New York");
        response.Data.AddressType.Should().Be(AddressType.Shipping);
        response.Data.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        response.Data.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetAddressById_WithValidId_ShouldReturnCorrectData()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create a test address with specific data
        var createRequest = AddressTestDataV1.Creation.CreateShippingAddressRequest(isDefault: true);
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(addressId);
        response.Data.AddressType.Should().Be(AddressType.Shipping);
        response.Data.IsDefault.Should().BeTrue();
        response.Data.UserId.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(AddressType.Shipping)]
    [InlineData(AddressType.Billing)]
    [InlineData(AddressType.Both)]
    public async Task GetAddressById_WithDifferentTypes_ShouldReturnCorrectType(AddressType addressType)
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AddressTestDataV1.Creation.CreateValidRequest(
            addressLine1: $"Test {addressType} Address",
            addressType: addressType);
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.AddressType.Should().Be(addressType);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetAddressById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"v1/addresses/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("not found", "NotFound");
    }

    [Fact]
    public async Task GetAddressById_WithInvalidGuidFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await Client.GetAsync("v1/addresses/invalid-guid-format");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAddressById_WithEmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Act
        var response = await Client.GetAsync($"v1/addresses/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().ContainAny("Address ID", "cannot be empty");
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task GetAddressById_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var addressId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"v1/addresses/{addressId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAddressById_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        SetAuthenticationHeader("expired.token.value");
        var addressId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"v1/addresses/{addressId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task GetAddressById_AccessingOtherUserAddress_ShouldReturnNotFound()
    {
        // Arrange
        // Create address as customer
        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);

        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Try to access as manager (different user)
        var managerToken = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(managerToken);

        // Act
        var response = await Client.GetAsync($"v1/addresses/{addressId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAddressById_CustomerAccessingOwnAddress_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(addressId);
    }

    [Fact]
    public async Task GetAddressById_AdminAccessingUserAddress_ShouldReturnNotFound()
    {
        // Arrange
        // Create address as customer
        var customerToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(customerToken);

        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Try to access as admin
        var adminToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminToken);

        // Act
        var response = await Client.GetAsync($"v1/addresses/{addressId}");

        // Assert
        // Admin should not be able to access customer addresses unless they own them
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Optional Fields Tests

    [Fact]
    public async Task GetAddressById_WithoutPhone_ShouldReturnSuccessWithNullPhone()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AddressTestDataV1.Creation.CreateAddressWithoutPhone();
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Phone.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetAddressById_WithoutAddressLine2_ShouldReturnSuccessWithNullLine2()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AddressTestDataV1.Creation.CreateAddressWithoutLine2();
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.AddressLine2.Should().BeNullOrEmpty();
    }

    #endregion

    #region Unicode and Special Characters Tests

    [Fact]
    public async Task GetAddressById_WithUnicodeCharacters_ShouldReturnCorrectData()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AddressTestDataV1.EdgeCases.CreateRequestWithUnicodeCharacters();
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.AddressLine1.Should().Be("123 Café Münchën Street™");
        response.Data.City.Should().Be("São Paulo");
        response.Data.State.Should().Be("Île-de-France");
    }

    [Fact]
    public async Task GetAddressById_WithSpecialCharacters_ShouldReturnCorrectData()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AddressTestDataV1.EdgeCases.CreateRequestWithSpecialCharacters();
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.AddressLine1.Should().Be("123 Main St. #456");
        response.Data.PostalCode.Should().Be("A1A-1A1");
    }

    #endregion

    #region Data Persistence Tests

    [Fact]
    public async Task GetAddressById_ShouldReturnDataFromDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AddressTestDataV1.Creation.CreateValidRequest(
            addressLine1: "456 Database Test Avenue",
            addressLine2: "Apt 789",
            city: "Los Angeles",
            state: "California",
            postalCode: "90001",
            country: "USA",
            phone: "+1234567890",
            addressType: AddressType.Both,
            isDefault: true);
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(response);

        // Verify data matches what was created
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(addressId);
        response.Data.AddressLine1.Should().Be("456 Database Test Avenue");
        response.Data.AddressLine2.Should().Be("Apt 789");
        response.Data.City.Should().Be("Los Angeles");
        response.Data.State.Should().Be("California");
        response.Data.PostalCode.Should().Be("90001");
        response.Data.Country.Should().Be("USA");
        response.Data.Phone.Should().Be("+1234567890");
        response.Data.AddressType.Should().Be(AddressType.Both);
        response.Data.IsDefault.Should().BeTrue();

        // Verify in database directly
        await ExecuteDbContextAsync(async context =>
        {
            var dbAddress = await context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId);

            dbAddress.Should().NotBeNull();
            dbAddress!.AddressLine1.Should().Be("456 Database Test Avenue");
            dbAddress.AddressLine2.Should().Be("Apt 789");
            dbAddress.City.Should().Be("Los Angeles");
            dbAddress.State.Should().Be("California");
            dbAddress.PostalCode.Should().Be("90001");
            dbAddress.Country.Should().Be("USA");
            dbAddress.Phone.Should().NotBeNull();
            dbAddress.Phone!.Value.Should().Be("+1234567890");
            dbAddress.AddressType.Should().Be(AddressType.Both);
            dbAddress.IsDefault.Should().BeTrue();
        });
    }

    #endregion

    #region Consistency Tests

    [Fact]
    public async Task GetAddressById_CalledTwice_ShouldReturnConsistentData()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act - Call twice
        var firstResponse = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");
        var secondResponse = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(firstResponse);
        AssertApiSuccess(secondResponse);

        firstResponse!.Data.Should().NotBeNull();
        secondResponse!.Data.Should().NotBeNull();

        // Data should be identical
        firstResponse.Data.Id.Should().Be(secondResponse.Data.Id);
        firstResponse.Data.AddressLine1.Should().Be(secondResponse.Data.AddressLine1);
        firstResponse.Data.AddressLine2.Should().Be(secondResponse.Data.AddressLine2);
        firstResponse.Data.City.Should().Be(secondResponse.Data.City);
        firstResponse.Data.State.Should().Be(secondResponse.Data.State);
        firstResponse.Data.PostalCode.Should().Be(secondResponse.Data.PostalCode);
        firstResponse.Data.Country.Should().Be(secondResponse.Data.Country);
        firstResponse.Data.Phone.Should().Be(secondResponse.Data.Phone);
        firstResponse.Data.AddressType.Should().Be(secondResponse.Data.AddressType);
        firstResponse.Data.IsDefault.Should().Be(secondResponse.Data.IsDefault);
        firstResponse.Data.CreatedAt.Should().Be(secondResponse.Data.CreatedAt);
        firstResponse.Data.UpdatedAt.Should().Be(secondResponse.Data.UpdatedAt);
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public async Task GetAddressById_ShouldReturnProperApiResponseFormat()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Message.Should().NotBeNullOrEmpty();
        response.Message.Should().Be("Address retrieved successfully");
        response.Errors.Should().BeEmpty();
        response.StatusCode.Should().Be(200);
    }

    #endregion

    #region All Address Types Integration Test

    [Fact]
    public async Task GetAddressById_AllSupportedTypes_ShouldReturnCorrectData()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var testCases = AddressTestDataV1.TypeSpecific.CreateAllAddressTypes();
        var addressIds = new List<Guid>();

        // Create all address types
        foreach (var request in testCases)
        {
            var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", request);
            AssertApiSuccess(createResponse);
            addressIds.Add(createResponse!.Data.Id);
        }

        // Act & Assert - Retrieve and verify each address
        var expectedTypes = new[] { AddressType.Shipping, AddressType.Billing, AddressType.Both };
        for (int i = 0; i < testCases.Count; i++)
        {
            var addressId = addressIds[i];
            var expectedType = expectedTypes[i];

            var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");
            AssertApiSuccess(response);

            response!.Data.Should().NotBeNull();
            response.Data.Id.Should().Be(addressId);
            response.Data.AddressType.Should().Be(expectedType);
        }
    }

    #endregion

    #region Default Address Tests

    [Fact]
    public async Task GetAddressById_DefaultAddress_ShouldReturnWithDefaultFlag()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AddressTestDataV1.DefaultManagement.CreateDefaultShippingAddress();
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.IsDefault.Should().BeTrue();
        response.Data.AddressType.Should().Be(AddressType.Shipping);
    }

    [Fact]
    public async Task GetAddressById_NonDefaultAddress_ShouldReturnWithoutDefaultFlag()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AddressTestDataV1.DefaultManagement.CreateNonDefaultAddress();
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.IsDefault.Should().BeFalse();
    }

    #endregion

    #region International Addresses Tests

    [Fact]
    public async Task GetAddressById_InternationalAddress_ShouldReturnCorrectData()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AddressTestDataV1.EdgeCases.CreateInternationalAddress();
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.City.Should().Be("Tokyo");
        response.Data.Country.Should().Be("Japan");
        response.Data.PostalCode.Should().Be("150-0002");
    }

    [Fact]
    public async Task GetAddressById_UKAddress_ShouldReturnCorrectData()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AddressTestDataV1.EdgeCases.CreateUKAddress();
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.AddressLine1.Should().Be("10 Downing Street");
        response.Data.City.Should().Be("London");
        response.Data.PostalCode.Should().Be("SW1A 2AA");
        response.Data.Country.Should().Be("United Kingdom");
    }

    [Fact]
    public async Task GetAddressById_CanadianAddress_ShouldReturnCorrectData()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var createRequest = AddressTestDataV1.EdgeCases.CreateCanadianAddress();
        var createResponse = await PostApiResponseAsync<object, AddressDto>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Act
        var response = await GetApiResponseAsync<AddressDto>($"v1/addresses/{addressId}");

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.City.Should().Be("Toronto");
        response.Data.State.Should().Be("Ontario");
        response.Data.PostalCode.Should().Be("M5H 2N2");
        response.Data.Country.Should().Be("Canada");
    }

    #endregion
}
