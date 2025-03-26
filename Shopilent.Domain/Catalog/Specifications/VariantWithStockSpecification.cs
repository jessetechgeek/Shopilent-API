using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Specifications;

public class VariantWithStockSpecification : Specification<ProductVariant>
{
    private readonly int _minimumStock;

    public VariantWithStockSpecification(int minimumStock = 1)
    {
        _minimumStock = minimumStock;
    }

    public override bool IsSatisfiedBy(ProductVariant variant)
    {
        return variant.IsActive && variant.StockQuantity >= _minimumStock;
    }
}