using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

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