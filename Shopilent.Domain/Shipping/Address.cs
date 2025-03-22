using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.Errors;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Shipping.Enums;
using Shopilent.Domain.Shipping.Errors;
using Shopilent.Domain.Shipping.Events;
using Shopilent.Domain.Shipping.ValueObjects;

namespace Shopilent.Domain.Shipping;

public class Address : AggregateRoot
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
        UserId = user.Id;
        PostalAddress = postalAddress;
        Phone = phone;
        IsDefault = isDefault;
        AddressType = addressType;
    }

    public static Result<Address> Create(
        User user,
        PostalAddress postalAddress,
        AddressType addressType = AddressType.Shipping,
        PhoneNumber phone = null,
        bool isDefault = false)
    {
        if (user == null)
            return Result.Failure<Address>(UserErrors.NotFound(Guid.Empty));

        if (postalAddress == null)
            return Result.Failure<Address>(AddressErrors.AddressLine1Required);

        var address = new Address(user, postalAddress, addressType, phone, isDefault);
        address.AddDomainEvent(new AddressCreatedEvent(address.Id, user.Id));
        return Result.Success(address);
    }

    public static Result<Address> CreateShipping(
        User user,
        PostalAddress postalAddress,
        PhoneNumber phone = null,
        bool isDefault = false)
    {
        return Create(user, postalAddress, AddressType.Shipping, phone, isDefault);
    }

    public static Result<Address> CreateBilling(
        User user,
        PostalAddress postalAddress,
        PhoneNumber phone = null,
        bool isDefault = false)
    {
        return Create(user, postalAddress, AddressType.Billing, phone, isDefault);
    }

    public static Result<Address> CreateDefaultAddress(
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

    public Result Update(
        PostalAddress postalAddress,
        PhoneNumber phone = null)
    {
        if (postalAddress == null)
            return Result.Failure(AddressErrors.AddressLine1Required);

        PostalAddress = postalAddress;
        Phone = phone;

        AddDomainEvent(new AddressUpdatedEvent(Id));
        return Result.Success();
    }

    public Result SetAddressType(AddressType addressType)
    {
        if (AddressType == addressType)
            return Result.Success();

        AddressType = addressType;

        AddDomainEvent(new AddressUpdatedEvent(Id));
        return Result.Success();
    }

    public Result SetDefault(bool isDefault)
    {
        if (IsDefault == isDefault)
            return Result.Success();

        IsDefault = isDefault;

        if (isDefault)
            AddDomainEvent(new DefaultAddressChangedEvent(Id, UserId, AddressType));

        AddDomainEvent(new AddressUpdatedEvent(Id));
        return Result.Success();
    }
}