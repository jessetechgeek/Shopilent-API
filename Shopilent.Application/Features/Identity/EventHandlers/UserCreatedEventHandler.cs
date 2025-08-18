using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Email;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Identity.Events;

namespace Shopilent.Application.Features.Identity.EventHandlers;

internal sealed  class UserCreatedEventHandler : INotificationHandler<DomainEventNotification<UserCreatedEvent>>
{
    private readonly ILogger<UserCreatedEventHandler> _logger;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _emailTemplateService;

    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger,
        IEmailService emailService,
        IEmailTemplateService emailTemplateService)
    {
        _logger = logger;
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
    }

    public Task Handle(DomainEventNotification<UserCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("User created with ID: {UserId}", domainEvent.UserId);

        return Task.CompletedTask;
    }
}
