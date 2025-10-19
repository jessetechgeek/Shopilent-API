using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Catalog.Events;

public class ProductCategoryAddedEvent : DomainEvent
{
    public ProductCategoryAddedEvent(Guid productId, Guid categoryId)
    {
        ProductId = productId;
        CategoryId = categoryId;
    }

    public Guid ProductId { get; }
    public Guid CategoryId { get; }
}