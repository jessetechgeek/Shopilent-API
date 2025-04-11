using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Catalog.Events;

public class CategoryDeletedEvent : DomainEvent
{
    public CategoryDeletedEvent(Guid categoryId)
    {
        CategoryId = categoryId;
    }

    public Guid CategoryId { get; }
}