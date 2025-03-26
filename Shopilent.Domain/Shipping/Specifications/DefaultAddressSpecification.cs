using Shopilent.Domain.Common;

namespace Shopilent.Domain.Shipping.Specifications;

public class DefaultAddressSpecification : Specification<Address>
{
    public override bool IsSatisfiedBy(Address address)
    {
        return address.IsDefault;
    }
}