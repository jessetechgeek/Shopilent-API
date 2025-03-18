using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Specifications;

public class ProductInCategorySpecification : Specification<Product>
{
    private readonly Guid _categoryId;

    public ProductInCategorySpecification(Guid categoryId)
    {
        _categoryId = categoryId;
    }

    public override bool IsSatisfiedBy(Product product)
    {
        return product.Categories.Any(pc => pc.CategoryId == _categoryId);
    }
}