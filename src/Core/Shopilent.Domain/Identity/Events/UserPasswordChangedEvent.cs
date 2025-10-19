using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Identity.Events;

public class UserPasswordChangedEvent : DomainEvent
{
    public UserPasswordChangedEvent(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}