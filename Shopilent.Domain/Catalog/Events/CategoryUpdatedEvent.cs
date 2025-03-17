using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Events;

public class CategoryUpdatedEvent : DomainEvent
{
    public CategoryUpdatedEvent(Guid categoryId)
    {
        CategoryId = categoryId;
    }

    public Guid CategoryId { get; }
}