using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Events;

public class ProductCreatedEvent : DomainEvent
{
    public ProductCreatedEvent(Guid productId)
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}