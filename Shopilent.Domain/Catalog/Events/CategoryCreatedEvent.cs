using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Events;

public class CategoryCreatedEvent : DomainEvent
{
    public CategoryCreatedEvent(Guid categoryId)
    {
        CategoryId = categoryId;
    }

    public Guid CategoryId { get; }
}