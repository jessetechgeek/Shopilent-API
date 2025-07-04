using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Sales.Events;

namespace Shopilent.Application.Features.Sales.EventHandlers;

public class CartAssignedToUserEventHandler : INotificationHandler<DomainEventNotification<CartAssignedToUserEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CartAssignedToUserEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public CartAssignedToUserEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<CartAssignedToUserEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<CartAssignedToUserEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Cart assigned to user. CartId: {CartId}, UserId: {UserId}",
            domainEvent.CartId,
            domainEvent.UserId);

        try
        {
            // Clear cart cache
            await _cacheService.RemoveAsync($"cart-{domainEvent.CartId}", cancellationToken);
            
            // Clear user cart cache
            await _cacheService.RemoveByPatternAsync($"user-{domainEvent.UserId}-cart", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CartAssignedToUserEvent for CartId: {CartId}, UserId: {UserId}",
                domainEvent.CartId, domainEvent.UserId);
        }
    }
}