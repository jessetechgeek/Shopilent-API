using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Sales.Events;

namespace Shopilent.Application.Features.Sales.EventHandlers;

public class CartClearedEventHandler : INotificationHandler<DomainEventNotification<CartClearedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CartClearedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public CartClearedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<CartClearedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<CartClearedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Cart cleared. CartId: {CartId}", domainEvent.CartId);

        try
        {
            // Clear cart cache
            await _cacheService.RemoveAsync($"cart-{domainEvent.CartId}", cancellationToken);
            
            // Get cart to check for user association
            var cart = await _unitOfWork.CartReader.GetByIdAsync(domainEvent.CartId, cancellationToken);
            
            if (cart != null && cart.UserId.HasValue)
            {
                // Clear user cart cache
                await _cacheService.RemoveByPatternAsync($"user-{cart.UserId.Value}-cart", cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CartClearedEvent for CartId: {CartId}", domainEvent.CartId);
        }
    }
}