using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Shipping.Events;

namespace Shopilent.Application.Features.Shipping.EventHandlers;

public class AddressCreatedEventHandler : INotificationHandler<DomainEventNotification<AddressCreatedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddressCreatedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public AddressCreatedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<AddressCreatedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<AddressCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Address created. AddressId: {AddressId}, UserId: {UserId}",
            domainEvent.AddressId,
            domainEvent.UserId);

        try
        {
            // Clear address caches
            await _cacheService.RemoveByPatternAsync("address-*", cancellationToken);
            await _cacheService.RemoveByPatternAsync($"user-addresses-{domainEvent.UserId}", cancellationToken);
            await _cacheService.RemoveByPatternAsync($"default-address-*-{domainEvent.UserId}", cancellationToken);
            
            // Get address details to check if it's a default address
            var address = await _unitOfWork.AddressReader.GetByIdAsync(domainEvent.AddressId, cancellationToken);
            
            if (address != null && address.IsDefault)
            {
                // If it's a default address, clear specific default address caches
                await _cacheService.RemoveByPatternAsync($"default-address-{address.AddressType}-{domainEvent.UserId}", 
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AddressCreatedEvent for AddressId: {AddressId}, UserId: {UserId}",
                domainEvent.AddressId, domainEvent.UserId);
        }
    }
}