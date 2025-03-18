using Shopilent.Domain.Common;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.Domain.Identity.Specifications;

public class AdminUserSpecification : Specification<User>
{
    public override bool IsSatisfiedBy(User user)
    {
        return user.Role == UserRole.Admin;
    }
}