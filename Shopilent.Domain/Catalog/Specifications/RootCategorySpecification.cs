using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Specifications;

public class RootCategorySpecification : Specification<Category>
{
    public override bool IsSatisfiedBy(Category category)
    {
        return category.ParentId == null;
    }
}