using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Email;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Payments.Events;

namespace Shopilent.Application.Features.Payments.EventHandlers;

public class PaymentMethodCreatedEventHandler : INotificationHandler<DomainEventNotification<PaymentMethodCreatedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentMethodCreatedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;
    private readonly IEmailService _emailService;

    public PaymentMethodCreatedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<PaymentMethodCreatedEventHandler> logger,
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

    public async Task Handle(DomainEventNotification<PaymentMethodCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Payment method created. PaymentMethodId: {PaymentMethodId}, UserId: {UserId}",
            domainEvent.PaymentMethodId,
            domainEvent.UserId);

        try
        {
            // Clear payment method caches
            await _cacheService.RemoveAsync($"payment-method-{domainEvent.PaymentMethodId}", cancellationToken);
            await _cacheService.RemoveByPatternAsync("payment-methods-*", cancellationToken);
            await _cacheService.RemoveByPatternAsync($"payment-methods-user-{domainEvent.UserId}", cancellationToken);
            
            // Get user details
            var user = await _unitOfWork.UserReader.GetByIdAsync(domainEvent.UserId, cancellationToken);
            
            if (user != null)
            {
                // Get payment method details
                var paymentMethod = await _unitOfWork.PaymentMethodReader.GetByIdAsync(domainEvent.PaymentMethodId, cancellationToken);
                
                if (paymentMethod != null)
                {
                    // Notify the user about the added payment method
                    string subject = "New Payment Method Added";
                    string message = $"A new payment method ({paymentMethod.DisplayName}) has been added to your account. " +
                                     "If you did not perform this action, please contact our support team immediately.";
                    
                    await _emailService.SendEmailAsync(user.Email, subject, message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PaymentMethodCreatedEvent for PaymentMethodId: {PaymentMethodId}, UserId: {UserId}",
                domainEvent.PaymentMethodId, domainEvent.UserId);
        }
    }
}