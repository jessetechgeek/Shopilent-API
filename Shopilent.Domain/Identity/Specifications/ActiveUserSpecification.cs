using Shopilent.Domain.Common;

namespace Shopilent.Domain.Identity.Specifications;

public class ActiveUserSpecification : Specification<User>
{
    public override bool IsSatisfiedBy(User user)
    {
        return user.IsActive;
    }
}