using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed  class ProductCategoryRemovedEventHandler : INotificationHandler<DomainEventNotification<ProductCategoryRemovedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductCategoryRemovedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public ProductCategoryRemovedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<ProductCategoryRemovedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<ProductCategoryRemovedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Product category removed. ProductId: {ProductId}, CategoryId: {CategoryId}",
            domainEvent.ProductId,
            domainEvent.CategoryId);

        try
        {
            // Invalidate product cache
            await _cacheService.RemoveAsync($"product-{domainEvent.ProductId}", cancellationToken);

            // Invalidate category product lists
            await _cacheService.RemoveByPatternAsync($"category-products-{domainEvent.CategoryId}", cancellationToken);

            // Invalidate product collections that might include this product
            await _cacheService.RemoveByPatternAsync("products-*", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ProductCategoryRemovedEvent for ProductId: {ProductId}, CategoryId: {CategoryId}",
                domainEvent.ProductId, domainEvent.CategoryId);
        }
    }
}
