using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.Domain.Identity.Events;

public class UserRoleChangedEvent : DomainEvent
{
    public UserRoleChangedEvent(Guid userId, UserRole newRole)
    {
        UserId = userId;
        NewRole = newRole;
    }

    public Guid UserId { get; }
    public UserRole NewRole { get; }
}