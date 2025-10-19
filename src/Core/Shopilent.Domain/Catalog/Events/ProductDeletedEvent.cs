using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Catalog.Events;

public class ProductDeletedEvent : DomainEvent
{
    public ProductDeletedEvent(Guid productId)
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}