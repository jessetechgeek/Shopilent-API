using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Specifications;

public class ActiveProductSpecification : Specification<Product>
{
    public override bool IsSatisfiedBy(Product product)
    {
        return product.IsActive;
    }
}