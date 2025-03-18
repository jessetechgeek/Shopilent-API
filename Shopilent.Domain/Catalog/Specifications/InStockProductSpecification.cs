using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Specifications;

public class InStockProductSpecification : Specification<Product>
{
    public override bool IsSatisfiedBy(Product product)
    {
        // If product has no variants, check if it has stock
        if (!product.Variants.Any())
        {
            return true; // Assuming base products don't track inventory
        }

        // Check if any variant has stock
        return product.Variants.Any(v => v.IsActive && v.StockQuantity > 0);
    }
}