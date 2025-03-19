using Shopilent.Domain.Common;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Shipping.Enums;
using Shopilent.Domain.Shipping.ValueObjects;

namespace Shopilent.Domain.Shipping;

public class Address : Entity
{
    private Address()
    {
        // Required by EF Core
    }

    private Address(
        User user,
        PostalAddress postalAddress,
        AddressType addressType = AddressType.Shipping,
        PhoneNumber phone = null,
        bool isDefault = false)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(postalAddress.AddressLine1))
            throw new ArgumentException("Address line 1 cannot be empty", nameof(postalAddress.AddressLine1));

        if (string.IsNullOrWhiteSpace(postalAddress.City))
            throw new ArgumentException("City cannot be empty", nameof(postalAddress.City));

        if (string.IsNullOrWhiteSpace(postalAddress.State))
            throw new ArgumentException("State cannot be empty", nameof(postalAddress.State));

        if (string.IsNullOrWhiteSpace(postalAddress.Country))
            throw new ArgumentException("Country cannot be empty", nameof(postalAddress.Country));

        if (string.IsNullOrWhiteSpace(postalAddress.PostalCode))
            throw new ArgumentException("Postal code cannot be empty", nameof(postalAddress.PostalCode));

        UserId = user.Id;
        PostalAddress = postalAddress;
        Phone = phone;
        IsDefault = isDefault;
        AddressType = addressType;
    }

    public static Address Create(
        User user,
        PostalAddress postalAddress,
        AddressType addressType = AddressType.Shipping,
        PhoneNumber phone = null,
        bool isDefault = false)
    {
        return new Address(user, postalAddress, addressType, phone,
            isDefault);
    }

    public static Address CreateShipping(
        User user,
        PostalAddress postalAddress,
        PhoneNumber phone = null,
        bool isDefault = false)
    {
        return Create(user, postalAddress, AddressType.Shipping, phone,
            isDefault);
    }

    public static Address CreateBilling(
        User user,
        PostalAddress postalAddress,
        PhoneNumber phone = null,
        bool isDefault = false)
    {
        return Create(user, postalAddress, AddressType.Billing, phone,
            isDefault);
    }

    public static Address CreateDefaultAddress(
        User user,
        PostalAddress postalAddress,
        AddressType addressType = AddressType.Both,
        PhoneNumber phone = null)
    {
        return Create(user, postalAddress, addressType, phone, true);
    }

    public Guid UserId { get; private set; }

    public PostalAddress PostalAddress { get; private set; }

    public string AddressLine1 => PostalAddress.AddressLine1;
    public string AddressLine2 => PostalAddress.AddressLine2;
    public string City => PostalAddress.City;
    public string State => PostalAddress.State;
    public string Country => PostalAddress.Country;
    public string PostalCode => PostalAddress.PostalCode;

    public PhoneNumber Phone { get; private set; }
    public bool IsDefault { get; private set; }
    public AddressType AddressType { get; private set; }

    public void Update(
        PostalAddress postalAddress,
        PhoneNumber phone = null)
    {
        if (string.IsNullOrWhiteSpace(postalAddress.AddressLine1))
            throw new ArgumentException("Address line 1 cannot be empty", nameof(postalAddress.AddressLine1));

        if (string.IsNullOrWhiteSpace(postalAddress.City))
            throw new ArgumentException("City cannot be empty", nameof(postalAddress.City));

        if (string.IsNullOrWhiteSpace(postalAddress.State))
            throw new ArgumentException("State cannot be empty", nameof(postalAddress.State));

        if (string.IsNullOrWhiteSpace(postalAddress.Country))
            throw new ArgumentException("Country cannot be empty", nameof(postalAddress.Country));

        if (string.IsNullOrWhiteSpace(postalAddress.PostalCode))
            throw new ArgumentException("Postal code cannot be empty", nameof(postalAddress.PostalCode));


        PostalAddress = postalAddress;
        Phone = phone;
    }

    public void SetAddressType(AddressType addressType)
    {
        AddressType = addressType;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }
}