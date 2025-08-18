using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Shipping.Events;

namespace Shopilent.Application.Features.Shipping.EventHandlers;

internal sealed class AddressUpdatedEventHandler : INotificationHandler<DomainEventNotification<AddressUpdatedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddressUpdatedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;

    public AddressUpdatedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<AddressUpdatedEventHandler> logger,
        ICacheService cacheService,
        IOutboxService outboxService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheService = cacheService;
        _outboxService = outboxService;
    }

    public async Task Handle(DomainEventNotification<AddressUpdatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Address updated. AddressId: {AddressId}", domainEvent.AddressId);

        try
        {
            // Get address details
            var address = await _unitOfWork.AddressReader.GetByIdAsync(domainEvent.AddressId, cancellationToken);

            if (address != null)
            {
                // Clear specific address cache
                await _cacheService.RemoveAsync($"address-{domainEvent.AddressId}", cancellationToken);

                // Clear user address caches
                await _cacheService.RemoveByPatternAsync($"user-addresses-{address.UserId}", cancellationToken);

                // If address type or default status has changed, clear related caches
                await _cacheService.RemoveByPatternAsync($"addresses-by-type-*-{address.UserId}", cancellationToken);

                if (address.IsDefault)
                {
                    await _cacheService.RemoveByPatternAsync($"default-address-*-{address.UserId}", cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AddressUpdatedEvent for AddressId: {AddressId}",
                domainEvent.AddressId);
        }
    }
}
