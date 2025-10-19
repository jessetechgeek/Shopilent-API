using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Domain.Catalog.Events;

public class AttributeCreatedEvent : DomainEvent
{
    public AttributeCreatedEvent(Guid attributeId)
    {
        AttributeId = attributeId;
    }

    public Guid AttributeId { get; }
}