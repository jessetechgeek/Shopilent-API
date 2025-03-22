using Shopilent.Domain.Common;
using Shopilent.Domain.Shipping.Enums;

namespace Shopilent.Domain.Shipping.Events;

public class DefaultAddressChangedEvent : DomainEvent
{
    public DefaultAddressChangedEvent(Guid addressId, Guid userId, AddressType addressType)
    {
        AddressId = addressId;
        UserId = userId;
        AddressType = addressType;
    }

    public Guid AddressId { get; }
    public Guid UserId { get; }
    public AddressType AddressType { get; }
}