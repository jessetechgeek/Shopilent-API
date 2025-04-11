using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Shipping.Events;

public class AddressDeletedEvent : DomainEvent
{
    public AddressDeletedEvent(Guid addressId, Guid userId)
    {
        AddressId = addressId;
        UserId = userId;
    }

    public Guid AddressId { get; }
    public Guid UserId { get; }
}