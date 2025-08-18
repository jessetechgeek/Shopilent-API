using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed  class ProductVariantStockChangedEventHandler : INotificationHandler<DomainEventNotification<ProductVariantStockChangedEvent>>
{
    private readonly ILogger<ProductVariantStockChangedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public ProductVariantStockChangedEventHandler(
        ILogger<ProductVariantStockChangedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<ProductVariantStockChangedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "Product variant stock changed. ProductId: {ProductId}, VariantId: {VariantId}, Old Quantity: {OldQuantity}, New Quantity: {NewQuantity}",
            domainEvent.ProductId,
            domainEvent.VariantId,
            domainEvent.OldQuantity,
            domainEvent.NewQuantity);

        // Invalidate product cache since stock levels affect availability
        await _cacheService.RemoveAsync($"product-{domainEvent.ProductId}", cancellationToken);

        // Invalidate variant-specific caches
        await _cacheService.RemoveByPatternAsync($"variant-{domainEvent.VariantId}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"product-variants-{domainEvent.ProductId}", cancellationToken);

        // Invalidate product listings that might show stock status
        await _cacheService.RemoveByPatternAsync("products-*", cancellationToken);

        // Check for low stock conditions
        bool isLowStock = domainEvent.NewQuantity > 0 && domainEvent.NewQuantity <= 5;
        bool isOutOfStock = domainEvent.NewQuantity == 0;
        bool isBackInStock = domainEvent.OldQuantity == 0 && domainEvent.NewQuantity > 0;
    }
}
