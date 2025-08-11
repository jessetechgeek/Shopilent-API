using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Email;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Identity.Events;

namespace Shopilent.Application.Features.Identity.EventHandlers;

public class UserPasswordChangedEventHandler : INotificationHandler<DomainEventNotification<UserPasswordChangedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserPasswordChangedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;
    private readonly IEmailService _emailService;

    public UserPasswordChangedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<UserPasswordChangedEventHandler> logger,
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

    public async Task Handle(DomainEventNotification<UserPasswordChangedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("User password changed. UserId: {UserId}", domainEvent.UserId);

        try
        {
            // Get user details
            var user = await _unitOfWork.UserReader.GetByIdAsync(domainEvent.UserId, cancellationToken);
            
            if (user != null)
            {
                // Revoke all active refresh tokens for this user
                var refreshTokens = await _unitOfWork.RefreshTokenReader.GetActiveTokensAsync(domainEvent.UserId, cancellationToken);
                if (refreshTokens != null && refreshTokens.Count > 0)
                {
                    foreach (var token in refreshTokens)
                    {
                        // Get the token from the write repository to revoke it
                        var refreshToken = await _unitOfWork.RefreshTokenWriter.GetByIdAsync(token.Id, cancellationToken);
                        if (refreshToken != null)
                        {
                            refreshToken.Revoke("Password changed");
                            await _unitOfWork.RefreshTokenWriter.UpdateAsync(refreshToken, cancellationToken);
                        }
                    }
                    
                    // Save changes to persist token revocations
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                
                // Send notification email
                string subject = "Your Password Has Been Changed";
                string message = $"Hi {user.FirstName},\n\n" +
                                 $"Your password for Shopilent has been successfully changed.\n\n" +
                                 $"If you did not request this change, please contact our support team immediately.\n\n" +
                                 $"The Shopilent Team";
                
                await _emailService.SendEmailAsync(user.Email, subject, message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserPasswordChangedEvent for UserId: {UserId}", domainEvent.UserId);
        }
    }
}