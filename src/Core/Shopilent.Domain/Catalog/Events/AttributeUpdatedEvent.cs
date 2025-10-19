using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Catalog.Events;

public class AttributeUpdatedEvent : DomainEvent
{
    public AttributeUpdatedEvent(Guid attributeId)
    {
        AttributeId = attributeId;
    }

    public Guid AttributeId { get; }
}