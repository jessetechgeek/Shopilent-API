using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Identity.Events;

namespace Shopilent.Application.Features.Identity.EventHandlers;

public class UserCreatedEventHandler : INotificationHandler<DomainEventNotification<UserCreatedEvent>>
{
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<UserCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("User created with ID: {UserId}", domainEvent.UserId);

        // Additional logic for handling user creation event
        // For example, sending welcome email, setting up default preferences, etc.

        return Task.CompletedTask;
    }
}