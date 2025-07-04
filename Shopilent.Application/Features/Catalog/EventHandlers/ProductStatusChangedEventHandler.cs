using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

public class ProductStatusChangedEventHandler : INotificationHandler<DomainEventNotification<ProductStatusChangedEvent>>
{
    private readonly ILogger<ProductStatusChangedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public ProductStatusChangedEventHandler(
        ILogger<ProductStatusChangedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<ProductStatusChangedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Product status changed for ID: {ProductId}. New status: {IsActive}",
            domainEvent.ProductId,
            domainEvent.IsActive ? "Active" : "Inactive");

        // Invalidate specific product cache
        await _cacheService.RemoveAsync($"product-{domainEvent.ProductId}", cancellationToken);

        // Also invalidate any collection caches
        await _cacheService.RemoveByPatternAsync("products-*", cancellationToken);
    }
}