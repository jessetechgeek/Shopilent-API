using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Identity.Events;

public class UserCreatedEvent : DomainEvent
{
    public UserCreatedEvent(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}