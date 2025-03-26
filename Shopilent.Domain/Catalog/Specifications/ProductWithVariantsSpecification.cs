using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Specifications;

public class ProductWithVariantsSpecification : Specification<Product>
{
    public override bool IsSatisfiedBy(Product product)
    {
        return product.Variants.Any();
    }
}