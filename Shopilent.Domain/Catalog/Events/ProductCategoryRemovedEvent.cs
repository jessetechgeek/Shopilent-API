using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Events;

public class ProductCategoryRemovedEvent : DomainEvent
{
    public ProductCategoryRemovedEvent(Guid productId, Guid categoryId)
    {
        ProductId = productId;
        CategoryId = categoryId;
    }

    public Guid ProductId { get; }
    public Guid CategoryId { get; }
}