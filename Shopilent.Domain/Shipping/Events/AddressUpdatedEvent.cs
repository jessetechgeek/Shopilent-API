using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Shipping.Events;

public class AddressUpdatedEvent : DomainEvent
{
    public AddressUpdatedEvent(Guid addressId)
    {
        AddressId = addressId;
    }

    public Guid AddressId { get; }
}