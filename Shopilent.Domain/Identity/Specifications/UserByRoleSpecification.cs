using Shopilent.Domain.Common;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.Domain.Identity.Specifications;

public class UserByRoleSpecification : Specification<User>
{
    private readonly UserRole _role;

    public UserByRoleSpecification(UserRole role)
    {
        _role = role;
    }

    public override bool IsSatisfiedBy(User user)
    {
        return user.Role == _role;
    }
}