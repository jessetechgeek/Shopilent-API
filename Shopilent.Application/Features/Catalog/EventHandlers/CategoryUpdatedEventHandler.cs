using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed  class CategoryUpdatedEventHandler : INotificationHandler<DomainEventNotification<CategoryUpdatedEvent>>
{
    private readonly ILogger<CategoryUpdatedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public CategoryUpdatedEventHandler(
        ILogger<CategoryUpdatedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<CategoryUpdatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Category updated with ID: {CategoryId}", domainEvent.CategoryId);

        // Invalidate specific category cache
        await _cacheService.RemoveAsync($"category-{domainEvent.CategoryId}", cancellationToken);

        // Also invalidate related categories caches
        await _cacheService.RemoveByPatternAsync("categories-*", cancellationToken);
        await _cacheService.RemoveByPatternAsync("all-categories", cancellationToken);
        await _cacheService.RemoveByPatternAsync("root-categories", cancellationToken);
        await _cacheService.RemoveByPatternAsync("child-categories-*", cancellationToken);
    }
}
