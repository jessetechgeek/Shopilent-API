using System.Net;
using Microsoft.EntityFrameworkCore;
using Shopilent.API.IntegrationTests.Common;
using Shopilent.API.IntegrationTests.Common.TestData;
using Shopilent.Application.Features.Shipping.Commands.CreateAddress.V1;
using Shopilent.Application.Features.Shipping.Commands.UpdateAddress.V1;
using Shopilent.Domain.Shipping.Enums;

namespace Shopilent.API.IntegrationTests.Endpoints.Shipping.UpdateAddress.V1;

public class UpdateAddressEndpointV1Tests : ApiIntegrationTestBase
{
    public UpdateAddressEndpointV1Tests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    #region Happy Path Tests

    [Fact]
    public async Task UpdateAddress_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest(
            addressLine1: "123 Original Street",
            city: "Original City",
            state: "Original State");
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Creation.CreateValidRequest(
            addressLine1: "456 Updated Avenue",
            city: "Updated City",
            state: "Updated State",
            postalCode: "54321",
            country: "Updated Country");

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(addressId);
        response.Data.AddressLine1.Should().Be("456 Updated Avenue");
        response.Data.City.Should().Be("Updated City");
        response.Data.State.Should().Be("Updated State");
        response.Data.PostalCode.Should().Be("54321");
        response.Data.Country.Should().Be("Updated Country");
        response.Data.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateAddress_WithValidData_ShouldUpdateAddressInDatabase()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest(
            addressLine1: "Original DB Street",
            city: "DB City");
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Creation.CreateValidRequest(
            addressLine1: "Updated DB Street",
            city: "Updated DB City",
            postalCode: "99999");

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);

        // Verify address updated in database
        await ExecuteDbContextAsync(async context =>
        {
            var address = await context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId);

            address.Should().NotBeNull();
            address!.AddressLine1.Should().Be("Updated DB Street");
            address.City.Should().Be("Updated DB City");
            address.PostalCode.Should().Be("99999");
            address.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        });
    }

    [Fact]
    public async Task UpdateAddress_ChangeAddressType_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create a shipping address
        var createRequest = AddressTestDataV1.Creation.CreateShippingAddressRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Creation.CreateBillingAddressRequest();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.AddressType.Should().Be(AddressType.Billing);
    }

    [Theory]
    [InlineData(AddressType.Shipping)]
    [InlineData(AddressType.Billing)]
    [InlineData(AddressType.Both)]
    public async Task UpdateAddress_WithAllValidAddressTypes_ShouldReturnSuccess(AddressType newAddressType)
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Creation.CreateValidRequest(addressType: newAddressType);

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.AddressType.Should().Be(newAddressType);
    }

    [Fact]
    public async Task UpdateAddress_WithoutPhone_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address with phone
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest(phone: "+1234567890");
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Creation.CreateAddressWithoutPhone();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Phone.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAddress_WithoutAddressLine2_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address with AddressLine2
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest(addressLine2: "Suite 100");
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Creation.CreateAddressWithoutLine2();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.AddressLine2.Should().BeNull();
    }

    #endregion

    #region Validation Tests - AddressLine1

    [Fact]
    public async Task UpdateAddress_WithEmptyAddressLine1_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithEmptyAddressLine1();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Address line 1 is required");
    }

    [Fact]
    public async Task UpdateAddress_WithNullAddressLine1_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithNullAddressLine1();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Address line 1 is required");
    }

    [Fact]
    public async Task UpdateAddress_WithWhitespaceAddressLine1_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithWhitespaceAddressLine1();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Address line 1 is required");
    }

    [Fact]
    public async Task UpdateAddress_WithLongAddressLine1_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        // UpdateAddress has 200 character limit, not 255 like CreateAddress
        var updateRequest = new
        {
            AddressLine1 = new string('A', 201), // Exceeds 200 character limit
            AddressLine2 = (string?)null,
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            Country = "Test Country",
            Phone = (string?)null,
            AddressType = AddressType.Shipping
        };

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("200 characters");
    }

    #endregion

    #region Validation Tests - AddressLine2

    [Fact]
    public async Task UpdateAddress_WithLongAddressLine2_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        // UpdateAddress has 200 character limit, not 255 like CreateAddress
        var updateRequest = new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = new string('B', 201), // Exceeds 200 character limit
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            Country = "Test Country",
            Phone = (string?)null,
            AddressType = AddressType.Shipping
        };

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("200 characters");
    }

    #endregion

    #region Validation Tests - City

    [Fact]
    public async Task UpdateAddress_WithEmptyCity_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithEmptyCity();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("City is required");
    }

    [Fact]
    public async Task UpdateAddress_WithNullCity_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithNullCity();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("City is required");
    }

    [Fact]
    public async Task UpdateAddress_WithLongCity_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithLongCity();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("100 characters");
    }

    #endregion

    #region Validation Tests - State

    [Fact]
    public async Task UpdateAddress_WithEmptyState_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithEmptyState();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("State is required");
    }

    [Fact]
    public async Task UpdateAddress_WithNullState_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithNullState();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("State is required");
    }

    [Fact]
    public async Task UpdateAddress_WithLongState_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithLongState();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("100 characters");
    }

    #endregion

    #region Validation Tests - PostalCode

    [Fact]
    public async Task UpdateAddress_WithEmptyPostalCode_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithEmptyPostalCode();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Postal code is required");
    }

    [Fact]
    public async Task UpdateAddress_WithNullPostalCode_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithNullPostalCode();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Postal code is required");
    }

    [Fact]
    public async Task UpdateAddress_WithLongPostalCode_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithLongPostalCode();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("20 characters");
    }

    #endregion

    #region Validation Tests - Country

    [Fact]
    public async Task UpdateAddress_WithEmptyCountry_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithEmptyCountry();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Country is required");
    }

    [Fact]
    public async Task UpdateAddress_WithNullCountry_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithNullCountry();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Country is required");
    }

    [Fact]
    public async Task UpdateAddress_WithLongCountry_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithLongCountry();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("100 characters");
    }

    #endregion

    #region Validation Tests - Phone

    [Fact]
    public async Task UpdateAddress_WithLongPhone_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithLongPhone();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("20 characters");
    }

    #endregion

    #region Validation Tests - AddressType

    [Fact]
    public async Task UpdateAddress_WithInvalidAddressType_ShouldReturnValidationError()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address first
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Validation.CreateRequestWithInvalidAddressType();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("Invalid address type");
    }

    #endregion

    #region Validation Tests - Not Found

    [Fact]
    public async Task UpdateAddress_WithInvalidAddressId_ShouldReturnNotFound()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var invalidId = Guid.NewGuid();
        var updateRequest = AddressTestDataV1.Creation.CreateValidRequest();

        // Act
        var response = await PutAsync($"v1/addresses/{invalidId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAddress_WithMalformedGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        var malformedId = "not-a-valid-guid";
        var updateRequest = AddressTestDataV1.Creation.CreateValidRequest();

        // Act
        var response = await PutAsync($"v1/addresses/{malformedId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Authentication & Authorization Tests

    [Fact]
    public async Task UpdateAddress_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthenticationHeader();
        var addressId = Guid.NewGuid();
        var updateRequest = AddressTestDataV1.Creation.CreateValidRequest();

        // Act
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAddress_WithCustomerRole_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "Customer Updated");

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.AddressLine1.Should().Be("Customer Updated");
    }

    [Fact]
    public async Task UpdateAddress_WithAdminRole_ShouldReturnSuccess()
    {
        // Arrange
        var adminAccessToken = await AuthenticateAsAdminAsync();
        SetAuthenticationHeader(adminAccessToken);

        // Create an address as admin
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "Admin Updated");

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.AddressLine1.Should().Be("Admin Updated");
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task UpdateAddress_WithMaximumLengthAddressLine1_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        // UpdateAddress has 200 character limit
        var updateRequest = new
        {
            AddressLine1 = new string('A', 200), // Exactly 200 characters
            AddressLine2 = (string?)null,
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            Country = "Test Country",
            Phone = (string?)null,
            AddressType = AddressType.Shipping
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.AddressLine1.Length.Should().Be(200);
    }

    [Fact]
    public async Task UpdateAddress_WithMaximumLengthAddressLine2_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        // UpdateAddress has 200 character limit
        var updateRequest = new
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = new string('B', 200), // Exactly 200 characters
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            Country = "Test Country",
            Phone = (string?)null,
            AddressType = AddressType.Shipping
        };

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.AddressLine2!.Length.Should().Be(200);
    }

    [Fact]
    public async Task UpdateAddress_WithMaximumLengthCity_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.BoundaryTests.CreateRequestWithMaximumLengthCity();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.City.Length.Should().Be(100);
    }

    [Fact]
    public async Task UpdateAddress_WithMaximumLengthState_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.BoundaryTests.CreateRequestWithMaximumLengthState();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.State.Length.Should().Be(100);
    }

    [Fact]
    public async Task UpdateAddress_WithMaximumLengthPostalCode_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.BoundaryTests.CreateRequestWithMaximumLengthPostalCode();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.PostalCode.Length.Should().Be(20);
    }

    [Fact]
    public async Task UpdateAddress_WithMaximumLengthCountry_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.BoundaryTests.CreateRequestWithMaximumLengthCountry();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Country.Length.Should().Be(100);
    }

    [Fact]
    public async Task UpdateAddress_WithMaximumLengthPhone_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.BoundaryTests.CreateRequestWithMaximumLengthPhone();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.Phone!.Length.Should().Be(20);
    }

    [Fact]
    public async Task UpdateAddress_WithMinimumValidAddress_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.BoundaryTests.CreateRequestWithMinimumValidAddress();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.AddressLine1.Should().Be("A");
        response.Data.City.Should().Be("B");
        response.Data.State.Should().Be("C");
        response.Data.PostalCode.Should().Be("1");
        response.Data.Country.Should().Be("D");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task UpdateAddress_WithUnicodeCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.EdgeCases.CreateRequestWithUnicodeCharacters();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.AddressLine1.Should().Contain("Café");
        response.Data.City.Should().Contain("São");
    }

    [Fact]
    public async Task UpdateAddress_WithSpecialCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.EdgeCases.CreateRequestWithSpecialCharacters();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.AddressLine1.Should().Contain("#");
        response.Data.PostalCode.Should().Contain("-");
    }

    [Fact]
    public async Task UpdateAddress_ToInternationalAddress_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.EdgeCases.CreateInternationalAddress();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.City.Should().Be("Tokyo");
        response.Data.Country.Should().Be("Japan");
    }

    [Fact]
    public async Task UpdateAddress_ToUKAddress_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.EdgeCases.CreateUKAddress();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.City.Should().Be("London");
        response.Data.Country.Should().Be("United Kingdom");
        response.Data.PostalCode.Should().Contain(" ");
    }

    [Fact]
    public async Task UpdateAddress_ToCanadianAddress_ShouldReturnSuccess()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create an address
        var createRequest = AddressTestDataV1.Creation.CreateValidRequest();
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);

        var addressId = createResponse!.Data.Id;
        var updateRequest = AddressTestDataV1.EdgeCases.CreateCanadianAddress();

        // Act
        var response = await PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{addressId}", updateRequest);

        // Assert
        AssertApiSuccess(response);
        response!.Data.City.Should().Be("Toronto");
        response.Data.Country.Should().Be("Canada");
    }

    #endregion

    #region Concurrent Update Tests

    [Fact]
    public async Task UpdateAddress_MultipleConcurrentUpdates_ShouldHandleGracefully()
    {
        // Arrange
        var accessToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(accessToken);

        // Create multiple addresses first
        var addressIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var createRequest = AddressTestDataV1.Creation.CreateValidRequest(
                addressLine1: $"Original Address {i}");
            var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
            AssertApiSuccess(createResponse);
            addressIds.Add(createResponse!.Data.Id);
        }

        // Create concurrent update tasks
        var tasks = addressIds.Select(id =>
        {
            var updateRequest = AddressTestDataV1.Creation.CreateValidRequest(
                addressLine1: $"Updated Address {id}");
            return PutApiResponseAsync<object, UpdateAddressResponseV1>($"v1/addresses/{id}", updateRequest);
        }).ToList();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => AssertApiSuccess(response));
        responses.Select(r => r!.Data.Id).Should().OnlyHaveUniqueItems();
        responses.Should().AllSatisfy(response =>
            response!.Data.AddressLine1.Should().StartWith("Updated Address"));
    }

    #endregion

    #region User Isolation Tests

    [Fact]
    public async Task UpdateAddress_OwnedByDifferentUser_ShouldReturnNotFound()
    {
        // Arrange - Create address as customer
        var firstUserToken = await AuthenticateAsCustomerAsync();
        SetAuthenticationHeader(firstUserToken);

        var createRequest = AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "Customer User Address");
        var createResponse = await PostApiResponseAsync<object, CreateAddressResponseV1>("v1/addresses", createRequest);
        AssertApiSuccess(createResponse);
        var addressId = createResponse!.Data.Id;

        // Create and authenticate as manager user (different user)
        var secondUserToken = await AuthenticateAsManagerAsync();
        SetAuthenticationHeader(secondUserToken);

        var updateRequest = AddressTestDataV1.Creation.CreateValidRequest(addressLine1: "Manager Updated Address");

        // Act - Try to update customer's address as manager
        var response = await PutAsync($"v1/addresses/{addressId}", updateRequest);

        // Assert - Should return 404 Not Found (as per handler implementation)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify original address is unchanged
        SetAuthenticationHeader(firstUserToken);
        await ExecuteDbContextAsync(async context =>
        {
            var address = await context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId);
            address.Should().NotBeNull();
            address!.AddressLine1.Should().Be("Customer User Address");
        });
    }

    #endregion

    // Response DTO for this specific endpoint version
    public class UpdateAddressResponseV1
    {
        public Guid Id { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public AddressType AddressType { get; set; }
        public bool IsDefault { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
