using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Email;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Identity.Events;

namespace Shopilent.Application.Features.Identity.EventHandlers;

internal sealed  class UserLockedOutEventHandler : INotificationHandler<DomainEventNotification<UserLockedOutEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserLockedOutEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;
    private readonly IEmailService _emailService;

    public UserLockedOutEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<UserLockedOutEventHandler> logger,
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

    public async Task Handle(DomainEventNotification<UserLockedOutEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("User locked out. UserId: {UserId}", domainEvent.UserId);

        try
        {
            // Clear user caches
            await _cacheService.RemoveAsync($"user-{domainEvent.UserId}", cancellationToken);
            await _cacheService.RemoveByPatternAsync("users-*", cancellationToken);

            // Get user details
            var user = await _unitOfWork.UserReader.GetByIdAsync(domainEvent.UserId, cancellationToken);

            if (user != null)
            {
                // Revoke all active refresh tokens for this user
                var refreshTokens =
                    await _unitOfWork.RefreshTokenReader.GetActiveTokensAsync(domainEvent.UserId, cancellationToken);
                if (refreshTokens != null && refreshTokens.Count > 0)
                {
                    foreach (var token in refreshTokens)
                    {
                        // Get the token from the write repository to revoke it
                        var refreshToken =
                            await _unitOfWork.RefreshTokenWriter.GetByIdAsync(token.Id, cancellationToken);
                        if (refreshToken != null)
                        {
                            refreshToken.Revoke("Account locked out");
                            await _unitOfWork.RefreshTokenWriter.UpdateAsync(refreshToken, cancellationToken);
                        }
                    }

                    // Save changes to persist token revocations
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                // Send lock notification email
                string subject = "Account Security Alert - Your Account Has Been Locked";
                string message = $"Hi {user.FirstName},\n\n" +
                                 $"Your account on Shopilent has been locked due to multiple failed login attempts.\n\n" +
                                 $"If this was you, you can reset your password using the 'Forgot Password' feature on the login page.\n\n" +
                                 $"If you did not attempt to log in, your account may have been targeted in an attack. " +
                                 $"Please contact our support team immediately.\n\n" +
                                 $"The Shopilent Team";

                await _emailService.SendEmailAsync(user.Email, subject, message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserLockedOutEvent for UserId: {UserId}", domainEvent.UserId);
        }
    }
}
