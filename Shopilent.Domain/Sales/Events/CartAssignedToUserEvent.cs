using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Sales.Events;

public class CartAssignedToUserEvent : DomainEvent
{
    public CartAssignedToUserEvent(Guid cartId, Guid userId)
    {
        CartId = cartId;
        UserId = userId;
    }

    public Guid CartId { get; }
    public Guid UserId { get; }
}