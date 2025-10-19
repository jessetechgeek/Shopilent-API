using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Identity.Commands.ResetPassword.V1;

internal sealed class ResetPasswordCommandHandlerV1 : ICommandHandler<ResetPasswordCommandV1>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<ResetPasswordCommandHandlerV1> _logger;

    public ResetPasswordCommandHandlerV1(
        IAuthenticationService authenticationService,
        ILogger<ResetPasswordCommandHandlerV1> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result> Handle(ResetPasswordCommandV1 request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate password match
            if (request.NewPassword != request.ConfirmPassword)
            {
                return Result.Failure(
                    Error.Validation(
                        code: "ResetPassword.PasswordMismatch",
                        message: "The password and confirmation password do not match."));
            }

            var result = await _authenticationService.ResetPasswordAsync(
                request.Token,
                request.NewPassword,
                cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError("Failed to reset password: {Error}", result.Error);
                return result;
            }

            _logger.LogInformation("Password reset successful");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when resetting password");
            
            return Result.Failure(
                Error.Failure(
                    code: "ResetPassword.Failed",
                    message: $"Failed to reset password: {ex.Message}"));
        }
    }
}