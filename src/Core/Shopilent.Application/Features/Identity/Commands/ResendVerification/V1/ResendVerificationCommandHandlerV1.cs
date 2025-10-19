using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Application.Features.Identity.Commands.ResendVerification.V1;

internal sealed class ResendVerificationCommandHandlerV1 : ICommandHandler<ResendVerificationCommandV1>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<ResendVerificationCommandHandlerV1> _logger;

    public ResendVerificationCommandHandlerV1(
        IAuthenticationService authenticationService,
        ILogger<ResendVerificationCommandHandlerV1> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result> Handle(ResendVerificationCommandV1 request, CancellationToken cancellationToken)
    {
        try
        {
            // Create email value object
            var emailResult = Email.Create(request.Email);
            if (emailResult.IsFailure)
                return Result.Failure(emailResult.Error);

            var result = await _authenticationService.SendEmailVerificationAsync(
                emailResult.Value,
                cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError("Failed to resend verification email: {Error}", result.Error);
                return result;
            }

            _logger.LogInformation("Verification email resent successfully to: {Email}", request.Email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when resending verification email for: {Email}", request.Email);
            
            return Result.Failure(
                Error.Failure(
                    code: "ResendVerification.Failed",
                    message: $"Failed to resend verification email: {ex.Message}"));
        }
    }
}