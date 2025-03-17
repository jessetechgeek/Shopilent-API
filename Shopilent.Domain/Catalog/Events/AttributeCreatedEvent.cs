using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.Events;

public class AttributeCreatedEvent : DomainEvent
{
    public AttributeCreatedEvent(Guid attributeId)
    {
        AttributeId = attributeId;
    }

    public Guid AttributeId { get; }
}