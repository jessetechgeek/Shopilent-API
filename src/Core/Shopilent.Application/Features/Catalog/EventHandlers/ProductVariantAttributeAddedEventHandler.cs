using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed  class ProductVariantAttributeAddedEventHandler : INotificationHandler<DomainEventNotification<ProductVariantAttributeAddedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductVariantAttributeAddedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public ProductVariantAttributeAddedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<ProductVariantAttributeAddedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<ProductVariantAttributeAddedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "Product variant attribute added. ProductId: {ProductId}, VariantId: {VariantId}, AttributeId: {AttributeId}",
            domainEvent.ProductId,
            domainEvent.VariantId,
            domainEvent.AttributeId);

        try
        {
            // Invalidate caches
            await _cacheService.RemoveAsync($"product-{domainEvent.ProductId}", cancellationToken);
            await _cacheService.RemoveAsync($"variant-{domainEvent.VariantId}", cancellationToken);
            await _cacheService.RemoveByPatternAsync($"product-variants-{domainEvent.ProductId}", cancellationToken);
            await _cacheService.RemoveAsync($"attribute-{domainEvent.AttributeId}", cancellationToken);
            await _cacheService.RemoveByPatternAsync("products-*", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing ProductVariantAttributeAddedEvent for ProductId: {ProductId}, VariantId: {VariantId}, AttributeId: {AttributeId}",
                domainEvent.ProductId,
                domainEvent.VariantId,
                domainEvent.AttributeId);
        }
    }
}
