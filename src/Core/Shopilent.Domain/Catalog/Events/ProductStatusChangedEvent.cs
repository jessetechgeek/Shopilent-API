using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Catalog.Events;

public class ProductStatusChangedEvent : DomainEvent
{
    public ProductStatusChangedEvent(Guid productId, bool isActive)
    {
        ProductId = productId;
        IsActive = isActive;
    }

    public Guid ProductId { get; }
    public bool IsActive { get; }
}