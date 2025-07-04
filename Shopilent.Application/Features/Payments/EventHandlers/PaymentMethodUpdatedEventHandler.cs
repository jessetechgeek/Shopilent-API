using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Payments.Events;

namespace Shopilent.Application.Features.Payments.EventHandlers;

public class PaymentMethodUpdatedEventHandler : INotificationHandler<DomainEventNotification<PaymentMethodUpdatedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentMethodUpdatedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public PaymentMethodUpdatedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<PaymentMethodUpdatedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<PaymentMethodUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Payment method updated. PaymentMethodId: {PaymentMethodId}", 
            domainEvent.PaymentMethodId);

        try
        {
            // Get payment method details to get user ID
            var paymentMethod = await _unitOfWork.PaymentMethodReader.GetByIdAsync(domainEvent.PaymentMethodId, cancellationToken);
            
            if (paymentMethod != null)
            {
                // Clear payment method caches
                await _cacheService.RemoveAsync($"payment-method-{domainEvent.PaymentMethodId}", cancellationToken);
                await _cacheService.RemoveByPatternAsync("payment-methods-*", cancellationToken);
                await _cacheService.RemoveByPatternAsync($"payment-methods-user-{paymentMethod.UserId}", cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PaymentMethodUpdatedEvent for PaymentMethodId: {PaymentMethodId}",
                domainEvent.PaymentMethodId);
        }
    }
}