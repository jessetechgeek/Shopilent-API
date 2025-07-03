using MediatR;
using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Features.Identity.Commands.Login.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity;

namespace Shopilent.Application.Features.Identity.Commands.RefreshToken.V1;

internal sealed class RefreshTokenCommandHandlerV1 : ICommandHandler<RefreshTokenCommandV1, RefreshTokenResponseV1>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<RefreshTokenCommandHandlerV1> _logger;

    public RefreshTokenCommandHandlerV1(IAuthenticationService authenticationService,
        ILogger<RefreshTokenCommandHandlerV1> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result<RefreshTokenResponseV1>> Handle(RefreshTokenCommandV1 request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authenticationService.RefreshTokenAsync(
                request.RefreshToken,
                request.IpAddress,
                request.UserAgent,
                cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError("Token refresh failed: {Error}", result.Error);
                return Result.Failure<RefreshTokenResponseV1>(result.Error);
            }

            var refreshTokenResponse = result.Value;
            var response = new RefreshTokenResponseV1
            {
                User = refreshTokenResponse.User,
                AccessToken = refreshTokenResponse.AccessToken,
                RefreshToken = refreshTokenResponse.RefreshToken,
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");

            return Result.Failure<RefreshTokenResponseV1>(
                Error.Failure(
                    code: "RefreshToken.Failed",
                    message: $"Token refresh failed: {ex.Message}"));
        }
    }
}