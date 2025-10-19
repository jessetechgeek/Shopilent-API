using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Events;

namespace Shopilent.Application.Abstractions.Events;

public interface IDomainEventService
{
    Task PublishAsync(DomainEvent domainEvent);
    Task ProcessEventAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);

}