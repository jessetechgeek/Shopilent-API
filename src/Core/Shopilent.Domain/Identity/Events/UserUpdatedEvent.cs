using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Identity.Events;

public class UserUpdatedEvent : DomainEvent
{
    public UserUpdatedEvent(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}