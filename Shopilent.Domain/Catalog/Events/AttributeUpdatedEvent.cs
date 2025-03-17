using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Events;

public class AttributeUpdatedEvent : DomainEvent
{
    public AttributeUpdatedEvent(Guid attributeId)
    {
        AttributeId = attributeId;
    }

    public Guid AttributeId { get; }
}