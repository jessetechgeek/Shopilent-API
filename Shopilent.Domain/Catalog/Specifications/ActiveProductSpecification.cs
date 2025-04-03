using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Specifications;

namespace Shopilent.Domain.Catalog.Specifications;

public class ActiveProductSpecification : Specification<Product>
{
    public override bool IsSatisfiedBy(Product product)
    {
        return product.IsActive;
    }
}