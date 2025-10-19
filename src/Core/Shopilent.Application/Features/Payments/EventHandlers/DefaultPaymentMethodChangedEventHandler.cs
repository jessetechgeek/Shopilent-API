using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Payments.Events;

namespace Shopilent.Application.Features.Payments.EventHandlers;

internal sealed  class DefaultPaymentMethodChangedEventHandler : INotificationHandler<DomainEventNotification<DefaultPaymentMethodChangedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DefaultPaymentMethodChangedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public DefaultPaymentMethodChangedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<DefaultPaymentMethodChangedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<DefaultPaymentMethodChangedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Default payment method changed. PaymentMethodId: {PaymentMethodId}, UserId: {UserId}",
            domainEvent.PaymentMethodId,
            domainEvent.UserId);

        try
        {
            // Clear default payment method cache
            await _cacheService.RemoveByPatternAsync($"default-payment-method-{domainEvent.UserId}", cancellationToken);

            // Clear all user payment methods cache as the default flag has changed
            await _cacheService.RemoveByPatternAsync($"payment-methods-user-{domainEvent.UserId}", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing DefaultPaymentMethodChangedEvent for PaymentMethodId: {PaymentMethodId}, UserId: {UserId}",
                domainEvent.PaymentMethodId, domainEvent.UserId);
        }
    }
}
