using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed  class AttributeCreatedEventHandler : INotificationHandler<DomainEventNotification<AttributeCreatedEvent>>
{
    private readonly ILogger<AttributeCreatedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public AttributeCreatedEventHandler(
        ILogger<AttributeCreatedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<AttributeCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Attribute created with ID: {AttributeId}", domainEvent.AttributeId);

        try
        {
            // Invalidate attribute caches
            await _cacheService.RemoveByPatternAsync("all-attributes", cancellationToken);

            // If it's a variant attribute, also invalidate the variant attributes cache
            await _cacheService.RemoveByPatternAsync("variant-attributes", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AttributeCreatedEvent for AttributeId: {AttributeId}",
                domainEvent.AttributeId);
        }
    }
}
