using Shopilent.Domain.Common;

namespace Shopilent.Domain.Identity.Events;

public class UserUpdatedEvent : DomainEvent
{
    public UserUpdatedEvent(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}