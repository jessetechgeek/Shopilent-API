using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Events;

public class ProductUpdatedEvent : DomainEvent
{
    public ProductUpdatedEvent(Guid productId)
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}