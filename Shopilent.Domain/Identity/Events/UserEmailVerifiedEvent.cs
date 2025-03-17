using Shopilent.Domain.Common;

namespace Shopilent.Domain.Identity.Events;

public class UserEmailVerifiedEvent : DomainEvent
{
    public UserEmailVerifiedEvent(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}