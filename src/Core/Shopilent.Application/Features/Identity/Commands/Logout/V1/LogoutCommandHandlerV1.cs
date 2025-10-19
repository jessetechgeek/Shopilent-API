using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Identity.Commands.Logout.V1;

internal sealed class LogoutCommandHandlerV1 : ICommandHandler<LogoutCommandV1>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<LogoutCommandHandlerV1> _logger;

    public LogoutCommandHandlerV1(
        IAuthenticationService authenticationService,
        ILogger<LogoutCommandHandlerV1> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result> Handle(LogoutCommandV1 request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authenticationService.RevokeTokenAsync(
                request.RefreshToken,
                request.Reason,
                cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError("Logout failed: {Error}", result.Error);
                return Result.Failure(result.Error);
            }

            _logger.LogInformation("User successfully logged out");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            
            return Result.Failure(
                Error.Failure(
                    code: "Logout.Failed",
                    message: $"Logout failed: {ex.Message}"));
        }
    }
}