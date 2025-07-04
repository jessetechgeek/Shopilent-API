using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

public class AttributeUpdatedEventHandler : INotificationHandler<DomainEventNotification<AttributeUpdatedEvent>>
{
    private readonly ILogger<AttributeUpdatedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public AttributeUpdatedEventHandler(
        ILogger<AttributeUpdatedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<AttributeUpdatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Attribute updated with ID: {AttributeId}", domainEvent.AttributeId);

        try
        {
            // Invalidate attribute cache
            await _cacheService.RemoveAsync($"attribute-{domainEvent.AttributeId}", cancellationToken);

            // Also invalidate the collection of all attributes
            await _cacheService.RemoveByPatternAsync("all-attributes", cancellationToken);

            // Invalidate any product caches that might use this attribute
            await _cacheService.RemoveByPatternAsync("product-*", cancellationToken);
            await _cacheService.RemoveByPatternAsync("products-*", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AttributeUpdatedEvent for AttributeId: {AttributeId}",
                domainEvent.AttributeId);
        }
    }
}