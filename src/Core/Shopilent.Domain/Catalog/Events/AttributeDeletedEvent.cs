using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Catalog.Events;

public class AttributeDeletedEvent : DomainEvent
{
    public AttributeDeletedEvent(Guid attributeId)
    {
        AttributeId = attributeId;
    }

    public Guid AttributeId { get; }
}