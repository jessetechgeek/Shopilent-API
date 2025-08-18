using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Email;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Common.Models;
using Shopilent.Domain.Identity.Events;

namespace Shopilent.Application.Features.Identity.EventHandlers;

internal sealed  class UserStatusChangedEventHandler : INotificationHandler<DomainEventNotification<UserStatusChangedEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserStatusChangedEventHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOutboxService _outboxService;
    private readonly IEmailService _emailService;

    public UserStatusChangedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<UserStatusChangedEventHandler> logger,
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

    public async Task Handle(DomainEventNotification<UserStatusChangedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("User status changed. UserId: {UserId}, IsActive: {IsActive}",
            domainEvent.UserId,
            domainEvent.IsActive);

        try
        {
            // Clear user caches
            await _cacheService.RemoveAsync($"user-{domainEvent.UserId}", cancellationToken);
            await _cacheService.RemoveByPatternAsync("users-*", cancellationToken);

            // Get user details
            var user = await _unitOfWork.UserReader.GetByIdAsync(domainEvent.UserId, cancellationToken);

            if (user != null)
            {
                if (domainEvent.IsActive)
                {
                    // User account activated
                    string subject = "Your Account Has Been Activated";
                    string message = $"Hi {user.FirstName},\n\n" +
                                     $"Your account on Shopilent has been activated. You can now log in and use all features of our platform.\n\n" +
                                     $"If you have any questions, please contact our support team.\n\n" +
                                     $"The Shopilent Team";

                    await _emailService.SendEmailAsync(user.Email, subject, message);
                }
                else
                {
                    // User account deactivated
                    string subject = "Your Account Has Been Deactivated";
                    string message = $"Hi {user.FirstName},\n\n" +
                                     $"Your account on Shopilent has been deactivated.\n\n" +
                                     $"If you believe this is an error or would like to reactivate your account, " +
                                     $"please contact our support team.\n\n" +
                                     $"The Shopilent Team";

                    await _emailService.SendEmailAsync(user.Email, subject, message);

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
                                refreshToken.Revoke("Account deactivated");
                                await _unitOfWork.RefreshTokenWriter.UpdateAsync(refreshToken, cancellationToken);
                            }
                        }

                        // Save changes to persist token revocations
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserStatusChangedEvent for UserId: {UserId}", domainEvent.UserId);
        }
    }
}
