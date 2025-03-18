using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Specifications;

public class ProductPriceRangeSpecification : Specification<Product>
{
    private readonly decimal _minPrice;
    private readonly decimal _maxPrice;

    public ProductPriceRangeSpecification(decimal minPrice, decimal maxPrice)
    {
        _minPrice = minPrice;
        _maxPrice = maxPrice;
    }

    public override bool IsSatisfiedBy(Product product)
    {
        var price = product.BasePrice.Amount;
        return price >= _minPrice && price <= _maxPrice;
    }
}