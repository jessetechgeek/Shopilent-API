using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed  class
    ProductVariantCreatedEventHandler : INotificationHandler<DomainEventNotification<ProductVariantCreatedEvent>>
{
    private readonly ILogger<ProductVariantCreatedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public ProductVariantCreatedEventHandler(
        ILogger<ProductVariantCreatedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<ProductVariantCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Product variant created. ProductId: {ProductId}, VariantId: {VariantId}",
            domainEvent.ProductId, domainEvent.VariantId);

        // Invalidate product cache since variants are part of product detail
        await _cacheService.RemoveAsync($"product-{domainEvent.ProductId}", cancellationToken);

        // Invalidate any variant-specific caches if they exist
        await _cacheService.RemoveByPatternAsync($"variant-{domainEvent.VariantId}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"product-variants-{domainEvent.ProductId}", cancellationToken);
    }
}
