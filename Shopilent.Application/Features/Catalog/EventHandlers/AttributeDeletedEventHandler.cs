using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed class AttributeDeletedEventHandler : INotificationHandler<DomainEventNotification<AttributeDeletedEvent>>
{
    private readonly ILogger<AttributeDeletedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public AttributeDeletedEventHandler(
        ILogger<AttributeDeletedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<AttributeDeletedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Attribute deleted with ID: {AttributeId}", domainEvent.AttributeId);

        try
        {
            // Invalidate specific attribute cache
            await _cacheService.RemoveAsync($"attribute-{domainEvent.AttributeId}", cancellationToken);

            // Invalidate collection caches
            await _cacheService.RemoveByPatternAsync("all-attributes", cancellationToken);
            await _cacheService.RemoveByPatternAsync("variant-attributes", cancellationToken);

            // Invalidate any product caches that might reference this attribute
            await _cacheService.RemoveByPatternAsync("product-*", cancellationToken);
            await _cacheService.RemoveByPatternAsync("products-*", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AttributeDeletedEvent for AttributeId: {AttributeId}",
                domainEvent.AttributeId);
        }
    }
}