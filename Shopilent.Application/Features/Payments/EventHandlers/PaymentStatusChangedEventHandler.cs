using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Payments.Events;

namespace Shopilent.Application.Features.Payments.EventHandlers;

public class PaymentStatusChangedEventHandler : INotificationHandler<DomainEventNotification<PaymentStatusChangedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentStatusChangedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public PaymentStatusChangedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<PaymentStatusChangedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<PaymentStatusChangedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "Payment status changed. PaymentId: {PaymentId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            domainEvent.PaymentId,
            domainEvent.OldStatus,
            domainEvent.NewStatus);

        try
        {
            // Get payment details
            var payment = await _unitOfWork.PaymentReader.GetByIdAsync(domainEvent.PaymentId, cancellationToken);
            
            if (payment != null)
            {
                // Clear payment cache
                await _cacheService.RemoveAsync($"payment-{domainEvent.PaymentId}", cancellationToken);
                
                // Clear payment collections
                await _cacheService.RemoveByPatternAsync("payments-*", cancellationToken);
                
                // Clear related order caches
                await _cacheService.RemoveAsync($"order-{payment.OrderId}", cancellationToken);
                await _cacheService.RemoveByPatternAsync("orders-*", cancellationToken);
                
                // Clear payment status-specific caches
                await _cacheService.RemoveByPatternAsync($"payments-status-{domainEvent.OldStatus}", cancellationToken);
                await _cacheService.RemoveByPatternAsync($"payments-status-{domainEvent.NewStatus}", cancellationToken);
                
                // If user is associated, clear user-related payment caches
                if (payment.UserId.HasValue)
                {
                    await _cacheService.RemoveByPatternAsync($"user-{payment.UserId.Value}-payments", cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PaymentStatusChangedEvent for PaymentId: {PaymentId}",
                domainEvent.PaymentId);
        }
    }
}