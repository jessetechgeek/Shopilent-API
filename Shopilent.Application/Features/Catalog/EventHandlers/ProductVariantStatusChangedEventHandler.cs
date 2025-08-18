using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed  class ProductVariantStatusChangedEventHandler : INotificationHandler<DomainEventNotification<ProductVariantStatusChangedEvent>>
{
    private readonly ILogger<ProductVariantStatusChangedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public ProductVariantStatusChangedEventHandler(
        ILogger<ProductVariantStatusChangedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<ProductVariantStatusChangedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "Product variant status changed. ProductId: {ProductId}, VariantId: {VariantId}, IsActive: {IsActive}",
            domainEvent.ProductId,
            domainEvent.VariantId,
            domainEvent.IsActive);

        // Invalidate product and variant caches
        await _cacheService.RemoveAsync($"product-{domainEvent.ProductId}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"variant-{domainEvent.VariantId}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"product-variants-{domainEvent.ProductId}", cancellationToken);

        // Invalidate product listings
        await _cacheService.RemoveByPatternAsync("products-*", cancellationToken);
    }
}
