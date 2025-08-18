using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed  class
    ProductVariantAttributeUpdatedEventHandler : INotificationHandler<
    DomainEventNotification<ProductVariantAttributeUpdatedEvent>>
{
    private readonly ILogger<ProductVariantAttributeUpdatedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public ProductVariantAttributeUpdatedEventHandler(
        ILogger<ProductVariantAttributeUpdatedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<ProductVariantAttributeUpdatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "Product variant attribute updated. ProductId: {ProductId}, VariantId: {VariantId}, AttributeId: {AttributeId}",
            domainEvent.ProductId,
            domainEvent.VariantId,
            domainEvent.AttributeId);

        try
        {
            // Invalidate product cache
            await _cacheService.RemoveAsync($"product-{domainEvent.ProductId}", cancellationToken);

            // Invalidate variant cache
            await _cacheService.RemoveAsync($"variant-{domainEvent.VariantId}", cancellationToken);

            // Invalidate product variants cache
            await _cacheService.RemoveByPatternAsync($"product-variants-{domainEvent.ProductId}", cancellationToken);

            // Invalidate attribute cache
            await _cacheService.RemoveAsync($"attribute-{domainEvent.AttributeId}", cancellationToken);

            // Since attribute changes can affect product filtering, also invalidate product collections
            await _cacheService.RemoveByPatternAsync("products-*", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing ProductVariantAttributeUpdatedEvent for ProductId: {ProductId}, VariantId: {VariantId}, AttributeId: {AttributeId}",
                domainEvent.ProductId,
                domainEvent.VariantId,
                domainEvent.AttributeId);
        }
    }
}
