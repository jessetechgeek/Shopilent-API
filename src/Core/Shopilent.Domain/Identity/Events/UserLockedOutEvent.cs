using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Identity.Events;

public class UserLockedOutEvent : DomainEvent
{
    public UserLockedOutEvent(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}