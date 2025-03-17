using Shopilent.Domain.Common;

namespace Shopilent.Domain.Identity.Events;

public class UserStatusChangedEvent : DomainEvent
{
    public UserStatusChangedEvent(Guid userId, bool isActive)
    {
        UserId = userId;
        IsActive = isActive;
    }

    public Guid UserId { get; }
    public bool IsActive { get; }
}