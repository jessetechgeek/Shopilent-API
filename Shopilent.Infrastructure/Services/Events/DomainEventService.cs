using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Events;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Common.Events;
using Shopilent.Domain.Outbox;

namespace Shopilent.Infrastructure.Services.Events;

public class DomainEventService : IDomainEventService
{
    // private readonly IPublisher _mediator;
    // private readonly ILogger<DomainEventService> _logger;
    //
    // public DomainEventService(
    //     IPublisher mediator,
    //     ILogger<DomainEventService> logger)
    // {
    //     _mediator = mediator;
    //     _logger = logger;
    // }
    // private readonly IPublisher _mediator;
    // private readonly IOutboxService _outboxService;
    // private readonly ILogger<DomainEventService> _logger;
    //
    // public DomainEventService(
    //     IPublisher mediator,
    //     IOutboxService outboxService,  // Inject outbox service
    //     ILogger<DomainEventService> logger)
    // {
    //     _mediator = mediator;
    //     _outboxService = outboxService;
    //     _logger = logger;
    // }
    
    private readonly IPublisher _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DomainEventService> _logger;
    
    public DomainEventService(
        IPublisher mediator,
        IUnitOfWork unitOfWork,
        ILogger<DomainEventService> logger)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // public async Task PublishAsync(DomainEvent domainEvent)
    // {
    //     _logger.LogInformation("Publishing domain event. Event - {event}", domainEvent.GetType().Name);
    //     
    //     // Create a generic notification type for this domain event
    //     Type notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
    //     
    //     // Create an instance of the notification with the domain event
    //     object notification = Activator.CreateInstance(notificationType, domainEvent);
    //     
    //     // Publish the notification
    //     await _mediator.Publish(notification);
    // }
    // public async Task PublishAsync(DomainEvent domainEvent)
    // {
    //     _logger.LogInformation("Publishing domain event. Event - {event}", domainEvent.GetType().Name);
    //     
    //     // Create a generic notification type for this domain event
    //     Type notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
    //     
    //     // Create an instance of the notification with the domain event
    //     object notification = Activator.CreateInstance(notificationType, domainEvent);
    //     
    //     // Save to outbox instead of direct publish
    //     await _outboxService.PublishAsync(notification);
    // }
    
    public async Task PublishAsync(DomainEvent domainEvent)
    {
        _logger.LogInformation("Publishing domain event from outbox. Event - {event}", domainEvent.GetType().Name);
        
        // Create a generic notification type for this domain event
        Type notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        
        // Create an instance of the notification with the domain event
        object notification = Activator.CreateInstance(notificationType, domainEvent);
        
        // Publish immediately using the mediator
        await _mediator.Publish(notification);
    }
    
    public async Task ProcessEventAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing domain event. Event - {event}", domainEvent.GetType().Name);
        
        // Create a generic notification type for this domain event
        Type notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        
        // Create an instance of the notification with the domain event
        object notification = Activator.CreateInstance(notificationType, domainEvent);
        
        // Create an outbox message and add it to the current context
        var outboxMessage = OutboxMessage.Create(notification);
        _unitOfWork.OutboxMessageWriteWriter.AddAsync(outboxMessage);
        // dbContext.Set<OutboxMessage>().Add();
    }
}