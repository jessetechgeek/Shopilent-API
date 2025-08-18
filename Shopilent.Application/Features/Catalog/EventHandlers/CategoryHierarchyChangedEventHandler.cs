using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

internal sealed class
    CategoryHierarchyChangedEventHandler : INotificationHandler<DomainEventNotification<CategoryHierarchyChangedEvent>>
{
    private readonly ILogger<CategoryHierarchyChangedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public CategoryHierarchyChangedEventHandler(
        ILogger<CategoryHierarchyChangedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<CategoryHierarchyChangedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Category hierarchy changed for ID: {CategoryId}", domainEvent.CategoryId);

        // Since hierarchy change affects many categories and their paths, clear all category caches
        await _cacheService.RemoveByPatternAsync("category-*", cancellationToken);
        await _cacheService.RemoveByPatternAsync("categories-*", cancellationToken);
        await _cacheService.RemoveByPatternAsync("root-categories", cancellationToken);
        await _cacheService.RemoveByPatternAsync("child-categories-*", cancellationToken);
    }
}
