using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Application.Features.Identity.Commands.ForgotPassword.V1;

internal sealed class ForgotPasswordCommandHandlerV1 : ICommandHandler<ForgotPasswordCommandV1>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<ForgotPasswordCommandHandlerV1> _logger;

    public ForgotPasswordCommandHandlerV1(
        IAuthenticationService authenticationService,
        ILogger<ForgotPasswordCommandHandlerV1> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommandV1 request, CancellationToken cancellationToken)
    {
        try
        {
            // Create email value object
            var emailResult = Email.Create(request.Email);
            if (emailResult.IsFailure)
                return Result.Failure(emailResult.Error);

            var result = await _authenticationService.RequestPasswordResetAsync(
                emailResult.Value,
                cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError("Failed to process forgot password request: {Error}", result.Error);
                return result;
            }

            _logger.LogInformation("Password reset email sent to: {Email}", request.Email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when processing forgot password request for: {Email}", request.Email);
            
            return Result.Failure(
                Error.Failure(
                    code: "ForgotPassword.Failed",
                    message: $"Failed to process password reset request: {ex.Message}"));
        }
    }
}