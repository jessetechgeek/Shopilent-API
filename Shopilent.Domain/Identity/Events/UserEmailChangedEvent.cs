using Shopilent.Domain.Common;

namespace Shopilent.Domain.Identity.Events;

public class UserEmailChangedEvent : DomainEvent
{
    public UserEmailChangedEvent(Guid userId, string newEmail)
    {
        UserId = userId;
        NewEmail = newEmail;
    }

    public Guid UserId { get; }
    public string NewEmail { get; }
}