using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Specifications;

namespace Shopilent.Domain.Shipping.Specifications;

public class DefaultAddressSpecification : Specification<Address>
{
    public override bool IsSatisfiedBy(Address address)
    {
        return address.IsDefault;
    }
}