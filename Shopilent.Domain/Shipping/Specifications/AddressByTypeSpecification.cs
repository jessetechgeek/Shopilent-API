using Shopilent.Domain.Common;
using Shopilent.Domain.Shipping.Enums;

namespace Shopilent.Domain.Shipping.Specifications;

public class AddressByTypeSpecification : Specification<Address>
{
    private readonly AddressType _addressType;

    public AddressByTypeSpecification(AddressType addressType)
    {
        _addressType = addressType;
    }

    public override bool IsSatisfiedBy(Address address)
    {
        if (_addressType == AddressType.Both)
            return address.AddressType == AddressType.Both;
            
        // For Shipping or Billing, also match if the address is of type Both
        return address.AddressType == _addressType || address.AddressType == AddressType.Both;
    }
}