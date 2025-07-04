using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.Endpoints.Users.ChangeUserRole.V1;

public class ChangeUserRoleRequestV1
{
    public UserRole Role { get; init; }
}