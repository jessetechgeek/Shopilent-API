using Shopilent.Domain.Common;

namespace Shopilent.Domain.Shipping.Events;

public class AddressCreatedEvent : DomainEvent
{
    public AddressCreatedEvent(Guid addressId, Guid userId)
    {
        AddressId = addressId;
        UserId = userId;
    }

    public Guid AddressId { get; }
    public Guid UserId { get; }
}