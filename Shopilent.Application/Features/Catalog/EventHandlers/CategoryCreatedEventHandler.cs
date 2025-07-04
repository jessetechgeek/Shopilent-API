using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Catalog.Events;

namespace Shopilent.Application.Features.Catalog.EventHandlers;

public class CategoryCreatedEventHandler : INotificationHandler<DomainEventNotification<CategoryCreatedEvent>>
{
    private readonly ILogger<CategoryCreatedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public CategoryCreatedEventHandler(
        ILogger<CategoryCreatedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<CategoryCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Category created with ID: {CategoryId}", domainEvent.CategoryId);

        // Invalidate categories cache
        await _cacheService.RemoveByPatternAsync("category-*", cancellationToken);
        await _cacheService.RemoveByPatternAsync("categories-*", cancellationToken);
        await _cacheService.RemoveByPatternAsync("root-categories", cancellationToken);
        await _cacheService.RemoveByPatternAsync("child-categories-*", cancellationToken);
    }
}