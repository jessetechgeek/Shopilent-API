using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

public class
    CategoryStatusChangedEventHandler : INotificationHandler<DomainEventNotification<CategoryStatusChangedEvent>>
{
    private readonly ILogger<CategoryStatusChangedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public CategoryStatusChangedEventHandler(
        ILogger<CategoryStatusChangedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<CategoryStatusChangedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Category status changed for ID: {CategoryId}. New status: {IsActive}",
            domainEvent.CategoryId,
            domainEvent.IsActive ? "Active" : "Inactive");

        // Invalidate specific category cache
        await _cacheService.RemoveAsync($"category-{domainEvent.CategoryId}", cancellationToken);

        // Also invalidate collection caches
        await _cacheService.RemoveByPatternAsync("categories-*", cancellationToken);
        await _cacheService.RemoveByPatternAsync("root-categories", cancellationToken);
    }
}