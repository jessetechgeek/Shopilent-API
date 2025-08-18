using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed  class
    ProductVariantUpdatedEventHandler : INotificationHandler<DomainEventNotification<ProductVariantUpdatedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductVariantUpdatedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public ProductVariantUpdatedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<ProductVariantUpdatedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<ProductVariantUpdatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Product variant updated. ProductId: {ProductId}, VariantId: {VariantId}",
            domainEvent.ProductId,
            domainEvent.VariantId);

        try
        {
            // Invalidate product cache since variants are part of product detail
            await _cacheService.RemoveAsync($"product-{domainEvent.ProductId}", cancellationToken);

            // Invalidate variant-specific caches
            await _cacheService.RemoveAsync($"variant-{domainEvent.VariantId}", cancellationToken);
            await _cacheService.RemoveByPatternAsync($"product-variants-{domainEvent.ProductId}", cancellationToken);

            // Also invalidate product listings that might include this product's variants
            await _cacheService.RemoveByPatternAsync("products-*", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing ProductVariantUpdatedEvent for ProductId: {ProductId}, VariantId: {VariantId}",
                domainEvent.ProductId, domainEvent.VariantId);
        }
    }
}
