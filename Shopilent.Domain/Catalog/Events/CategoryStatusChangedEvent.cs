using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Catalog.Events;

public class CategoryStatusChangedEvent : DomainEvent
{
    public CategoryStatusChangedEvent(Guid categoryId, bool isActive)
    {
        CategoryId = categoryId;
        IsActive = isActive;
    }

    public Guid CategoryId { get; }
    public bool IsActive { get; }
}