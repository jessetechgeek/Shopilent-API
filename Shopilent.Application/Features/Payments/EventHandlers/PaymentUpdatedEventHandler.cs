using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Payments.Events;

namespace Shopilent.Application.Features.Payments.EventHandlers;

public class PaymentUpdatedEventHandler : INotificationHandler<DomainEventNotification<PaymentUpdatedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentUpdatedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public PaymentUpdatedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<PaymentUpdatedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<PaymentUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Payment updated. PaymentId: {PaymentId}", domainEvent.PaymentId);

        try
        {
            // Clear payment cache
            await _cacheService.RemoveAsync($"payment-{domainEvent.PaymentId}", cancellationToken);
            
            // Clear payment collections
            await _cacheService.RemoveByPatternAsync("payments-*", cancellationToken);
            
            // Get payment details to clear more specific caches
            var payment = await _unitOfWork.PaymentReader.GetByIdAsync(domainEvent.PaymentId, cancellationToken);
            
            if (payment != null)
            {
                // Clear related order caches
                await _cacheService.RemoveAsync($"order-{payment.OrderId}", cancellationToken);
                
                // If user is associated, clear user-related payment caches
                if (payment.UserId.HasValue)
                {
                    await _cacheService.RemoveByPatternAsync($"user-{payment.UserId.Value}-payments", cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PaymentUpdatedEvent for PaymentId: {PaymentId}",
                domainEvent.PaymentId);
        }
    }
}