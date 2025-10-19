using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Sales.Events;

namespace Shopilent.Application.Features.Sales.EventHandlers;

internal sealed  class OrderItemRemovedEventHandler : INotificationHandler<DomainEventNotification<OrderItemRemovedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderItemRemovedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public OrderItemRemovedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<OrderItemRemovedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<OrderItemRemovedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Order item removed. OrderId: {OrderId}, OrderItemId: {OrderItemId}",
            domainEvent.OrderId,
            domainEvent.OrderItemId);

        try
        {
            // Clear order cache
            await _cacheService.RemoveAsync($"order-{domainEvent.OrderId}", cancellationToken);

            // Clear order item cache
            await _cacheService.RemoveAsync($"order-item-{domainEvent.OrderItemId}", cancellationToken);

            // Clear order items collection cache
            await _cacheService.RemoveByPatternAsync($"order-items-{domainEvent.OrderId}", cancellationToken);

            // Get order details
            var order = await _unitOfWork.OrderReader.GetDetailByIdAsync(domainEvent.OrderId, cancellationToken);

            if (order != null && order.UserId.HasValue)
            {
                // Clear user orders cache
                await _cacheService.RemoveByPatternAsync($"user-{order.UserId.Value}-orders", cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderItemRemovedEvent for OrderId: {OrderId}, OrderItemId: {OrderItemId}",
                domainEvent.OrderId, domainEvent.OrderItemId);
        }
    }
}
