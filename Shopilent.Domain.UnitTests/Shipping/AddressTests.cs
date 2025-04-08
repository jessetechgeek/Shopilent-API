using Shopilent.Domain.Shipping;
using Shopilent.Domain.Shipping.Enums;
using Shopilent.Domain.Shipping.Events;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Shipping.ValueObjects;

namespace Shopilent.Domain.Tests.Shipping;

public class AddressTests
{
    private User CreateTestUser()
    {
        var emailResult = Email.Create("test@example.com");
        Assert.True(emailResult.IsSuccess);

        var fullNameResult = FullName.Create("Test", "User");
        Assert.True(fullNameResult.IsSuccess);

        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);

        Assert.True(userResult.IsSuccess);
        return userResult.Value;
    }

    [Fact]
    public void Create_WithValidParameters_ShouldCreateAddress()
    {
        // Arrange
        var user = CreateTestUser();

        var postalAddressResult = PostalAddress.Create(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345",
            "Apt 4B");

        Assert.True(postalAddressResult.IsSuccess);
        var postalAddress = postalAddressResult.Value;

        var addressType = AddressType.Shipping;

        var phoneResult = PhoneNumber.Create("555-123-4567");
        Assert.True(phoneResult.IsSuccess);
        var phone = phoneResult.Value;

        var isDefault = true;

        // Act - Use CreateShipping instead of internal Create method
        var result = Address.CreateShipping(
            user,
            postalAddress,
            phone,
            isDefault);

        // Assert
        Assert.True(result.IsSuccess);
        var address = result.Value;
        Assert.Equal(user.Id, address.UserId);
        Assert.Equal(postalAddress, address.PostalAddress);
        Assert.Equal(postalAddress.AddressLine1, address.AddressLine1);
        Assert.Equal(postalAddress.AddressLine2, address.AddressLine2);
        Assert.Equal(postalAddress.City, address.City);
        Assert.Equal(postalAddress.State, address.State);
        Assert.Equal(postalAddress.Country, address.Country);
        Assert.Equal(postalAddress.PostalCode, address.PostalCode);
        Assert.Equal(phone, address.Phone);
        Assert.Equal(addressType, address.AddressType);
        Assert.Equal(isDefault, address.IsDefault);
        Assert.Contains(address.DomainEvents, e => e is AddressCreatedEvent);
    }

    [Fact]
    public void Create_WithNullUser_ShouldReturnFailure()
    {
        // Arrange
        User user = null;

        var postalAddressResult = PostalAddress.Create(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345");

        Assert.True(postalAddressResult.IsSuccess);
        var postalAddress = postalAddressResult.Value;

        // Act - Use CreateShipping instead of internal Create method
        var result = Address.CreateShipping(user, postalAddress);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.NotFound", result.Error.Code);
    }

    [Fact]
    public void Create_WithNullPostalAddress_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        PostalAddress postalAddress = null;

        // Act - Use CreateShipping instead of internal Create method
        var result = Address.CreateShipping(user, postalAddress);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Address.AddressLine1Required", result.Error.Code);
    }

    [Fact]
    public void CreateShipping_ShouldCreateShippingAddress()
    {
        // Arrange
        var user = CreateTestUser();

        var postalAddressResult = PostalAddress.Create(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345",
            "Suite 100");

        Assert.True(postalAddressResult.IsSuccess);
        var postalAddress = postalAddressResult.Value;

        var phoneResult = PhoneNumber.Create("555-123-4567");
        Assert.True(phoneResult.IsSuccess);
        var phone = phoneResult.Value;

        var isDefault = true;

        // Act
        var result = Address.CreateShipping(
            user,
            postalAddress,
            phone,
            isDefault);

        // Assert
        Assert.True(result.IsSuccess);
        var address = result.Value;
        Assert.Equal(user.Id, address.UserId);
        Assert.Equal(postalAddress, address.PostalAddress);
        Assert.Equal(phone, address.Phone);
        Assert.Equal(AddressType.Shipping, address.AddressType);
        Assert.Equal(isDefault, address.IsDefault);
        Assert.Contains(address.DomainEvents, e => e is AddressCreatedEvent);
    }

    [Fact]
    public void CreateBilling_ShouldCreateBillingAddress()
    {
        // Arrange
        var user = CreateTestUser();

        var postalAddressResult = PostalAddress.Create(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345",
            "Suite 100");

        Assert.True(postalAddressResult.IsSuccess);
        var postalAddress = postalAddressResult.Value;

        var phoneResult = PhoneNumber.Create("555-123-4567");
        Assert.True(phoneResult.IsSuccess);
        var phone = phoneResult.Value;

        var isDefault = true;

        // Act
        var result = Address.CreateBilling(
            user,
            postalAddress,
            phone,
            isDefault);

        // Assert
        Assert.True(result.IsSuccess);
        var address = result.Value;
        Assert.Equal(user.Id, address.UserId);
        Assert.Equal(postalAddress, address.PostalAddress);
        Assert.Equal(phone, address.Phone);
        Assert.Equal(AddressType.Billing, address.AddressType);
        Assert.Equal(isDefault, address.IsDefault);
        Assert.Contains(address.DomainEvents, e => e is AddressCreatedEvent);
    }

    [Fact]
    public void CreateDefaultAddress_ShouldCreateDefaultAddress()
    {
        // Arrange
        var user = CreateTestUser();

        var postalAddressResult = PostalAddress.Create(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345",
            "Suite 100");

        Assert.True(postalAddressResult.IsSuccess);
        var postalAddress = postalAddressResult.Value;

        var addressType = AddressType.Both;

        var phoneResult = PhoneNumber.Create("555-123-4567");
        Assert.True(phoneResult.IsSuccess);
        var phone = phoneResult.Value;

        // Act
        var result = Address.CreateDefaultAddress(
            user,
            postalAddress,
            addressType,
            phone);

        // Assert
        Assert.True(result.IsSuccess);
        var address = result.Value;
        Assert.Equal(user.Id, address.UserId);
        Assert.Equal(postalAddress, address.PostalAddress);
        Assert.Equal(phone, address.Phone);
        Assert.Equal(addressType, address.AddressType);
        Assert.True(address.IsDefault);
        Assert.Contains(address.DomainEvents, e => e is AddressCreatedEvent);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateAddress()
    {
        // Arrange
        var user = CreateTestUser();

        var originalPostalAddressResult = PostalAddress.Create(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345");

        Assert.True(originalPostalAddressResult.IsSuccess);
        var originalPostalAddress = originalPostalAddressResult.Value;

        var addressResult = Address.CreateShipping(user, originalPostalAddress);
        Assert.True(addressResult.IsSuccess);
        var address = addressResult.Value;

        var newPostalAddressResult = PostalAddress.Create(
            "456 New St",
            "Newtown",
            "NewState",
            "NewCountry",
            "56789",
            "Suite 100");

        Assert.True(newPostalAddressResult.IsSuccess);
        var newPostalAddress = newPostalAddressResult.Value;

        var newPhoneResult = PhoneNumber.Create("555-987-6543");
        Assert.True(newPhoneResult.IsSuccess);
        var newPhone = newPhoneResult.Value;

        // Act
        var updateResult = address.Update(newPostalAddress, newPhone);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(newPostalAddress, address.PostalAddress);
        Assert.Equal(newPostalAddress.AddressLine1, address.AddressLine1);
        Assert.Equal(newPostalAddress.AddressLine2, address.AddressLine2);
        Assert.Equal(newPostalAddress.City, address.City);
        Assert.Equal(newPostalAddress.State, address.State);
        Assert.Equal(newPostalAddress.Country, address.Country);
        Assert.Equal(newPostalAddress.PostalCode, address.PostalCode);
        Assert.Equal(newPhone, address.Phone);
        Assert.Contains(address.DomainEvents, e => e is AddressUpdatedEvent);
    }

    [Fact]
    public void Update_WithNullPostalAddress_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();

        var originalPostalAddressResult = PostalAddress.Create(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345");

        Assert.True(originalPostalAddressResult.IsSuccess);
        var originalPostalAddress = originalPostalAddressResult.Value;

        var addressResult = Address.CreateShipping(user, originalPostalAddress);
        Assert.True(addressResult.IsSuccess);
        var address = addressResult.Value;

        PostalAddress newPostalAddress = null;

        // Act
        var updateResult = address.Update(newPostalAddress);

        // Assert
        Assert.True(updateResult.IsFailure);
        Assert.Equal("Address.AddressLine1Required", updateResult.Error.Code);
    }

    [Fact]
    public void SetAddressType_ShouldUpdateAddressType()
    {
        // Arrange
        var user = CreateTestUser();

        var postalAddressResult = PostalAddress.Create(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345");

        Assert.True(postalAddressResult.IsSuccess);
        var postalAddress = postalAddressResult.Value;

        var addressResult = Address.CreateShipping(user, postalAddress);
        Assert.True(addressResult.IsSuccess);
        var address = addressResult.Value;
        Assert.Equal(AddressType.Shipping, address.AddressType);

        var newAddressType = AddressType.Both;

        // Act
        var result = address.SetAddressType(newAddressType);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newAddressType, address.AddressType);
        Assert.Contains(address.DomainEvents, e => e is AddressUpdatedEvent);
    }

    [Fact]
    public void SetDefault_ShouldUpdateIsDefault()
    {
        // Arrange
        var user = CreateTestUser();

        var postalAddressResult = PostalAddress.Create(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345");

        Assert.True(postalAddressResult.IsSuccess);
        var postalAddress = postalAddressResult.Value;

        var addressResult = Address.CreateShipping(user, postalAddress);
        Assert.True(addressResult.IsSuccess);
        var address = addressResult.Value;
        Assert.False(address.IsDefault);

        address.ClearDomainEvents();

        // Act
        var result = address.SetDefault(true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(address.IsDefault);
        Assert.Contains(address.DomainEvents, e => e is DefaultAddressChangedEvent);
        Assert.Contains(address.DomainEvents, e => e is AddressUpdatedEvent);

        // Act again
        address.ClearDomainEvents();
        var result2 = address.SetDefault(false);

        // Assert again
        Assert.True(result2.IsSuccess);
        Assert.False(address.IsDefault);
        Assert.DoesNotContain(address.DomainEvents, e => e is DefaultAddressChangedEvent);
        Assert.Contains(address.DomainEvents, e => e is AddressUpdatedEvent);
    }
}