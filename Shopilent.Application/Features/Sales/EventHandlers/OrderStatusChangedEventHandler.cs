using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Email;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Sales.Enums;
using Shopilent.Domain.Sales.Events;

namespace Shopilent.Application.Features.Sales.EventHandlers;

public class OrderStatusChangedEventHandler : INotificationHandler<DomainEventNotification<OrderStatusChangedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;
    private readonly IEmailService _emailService;

    public OrderStatusChangedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<OrderStatusChangedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
        _emailService = emailService;
    }

    public async Task Handle(DomainEventNotification<OrderStatusChangedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "Order status changed. OrderId: {OrderId}, Old Status: {OldStatus}, New Status: {NewStatus}",
            domainEvent.OrderId,
            domainEvent.OldStatus,
            domainEvent.NewStatus);

        try
        {
            // Clear order caches
            await _cacheService.RemoveAsync($"order-{domainEvent.OrderId}", cancellationToken);
            await _cacheService.RemoveByPatternAsync("orders-*", cancellationToken);

            // Get order details
            var order = await _unitOfWork.OrderReader.GetDetailByIdAsync(domainEvent.OrderId, cancellationToken);

            if (order != null && order.UserId.HasValue)
            {
                // Get user information
                var user = await _unitOfWork.UserReader.GetByIdAsync(order.UserId.Value, cancellationToken);

                if (user != null)
                {
                    // Send status update email based on the new status
                    string subject = $"Order #{order.Id} Status Update";
                    string message = "";

                    switch (domainEvent.NewStatus)
                    {
                        case OrderStatus.Processing:
                            message =
                                $"Your order #{order.Id} is now being processed. We'll notify you once it's shipped.";
                            break;
                        case OrderStatus.Shipped:
                            message = $"Good news! Your order #{order.Id} has been shipped.";
                            if (order.TrackingNumber != null)
                            {
                                message += $" You can track your package with tracking number: {order.TrackingNumber}";
                            }

                            break;
                        case OrderStatus.Delivered:
                            message = $"Your order #{order.Id} has been delivered. Thank you for shopping with us!";
                            break;
                        case OrderStatus.Cancelled:
                            message = $"Your order #{order.Id} has been cancelled.";
                            if (order.Metadata != null && order.Metadata.ContainsKey("cancellationReason"))
                            {
                                message += $" Reason: {order.Metadata["cancellationReason"]}";
                            }

                            break;
                    }

                    if (!string.IsNullOrEmpty(message))
                    {
                        await _emailService.SendEmailAsync(user.Email, subject, message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderStatusChangedEvent for OrderId: {OrderId}",
                domainEvent.OrderId);
        }
    }
}