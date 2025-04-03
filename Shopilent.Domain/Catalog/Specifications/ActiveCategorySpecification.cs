using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Specifications;

namespace Shopilent.Domain.Catalog.Specifications;

public class ActiveCategorySpecification : Specification<Category>
{
    public override bool IsSatisfiedBy(Category category)
    {
        return category.IsActive;
    }
}