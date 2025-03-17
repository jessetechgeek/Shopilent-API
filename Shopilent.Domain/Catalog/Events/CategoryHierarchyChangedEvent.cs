using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Events;

public class CategoryHierarchyChangedEvent : DomainEvent
{
    public CategoryHierarchyChangedEvent(Guid categoryId)
    {
        CategoryId = categoryId;
    }

    public Guid CategoryId { get; }
}