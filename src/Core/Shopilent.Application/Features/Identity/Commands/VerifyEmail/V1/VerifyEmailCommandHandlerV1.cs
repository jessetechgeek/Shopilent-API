using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Identity.Commands.VerifyEmail.V1;

internal sealed class VerifyEmailCommandHandlerV1 : ICommandHandler<VerifyEmailCommandV1>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<VerifyEmailCommandHandlerV1> _logger;

    public VerifyEmailCommandHandlerV1(
        IAuthenticationService authenticationService,
        ILogger<VerifyEmailCommandHandlerV1> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result> Handle(VerifyEmailCommandV1 request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return Result.Failure(
                    Error.Validation(
                        code: "VerifyEmail.TokenRequired",
                        message: "Verification token is required."));

            var result = await _authenticationService.VerifyEmailAsync(
                request.Token,
                cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError("Email verification failed: {Error}", result.Error);
                return result;
            }

            _logger.LogInformation("Email verification succeeded for token: {Token}", request.Token);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification for token: {Token}", request.Token);
            
            return Result.Failure(
                Error.Failure(
                    code: "VerifyEmail.Failed",
                    message: $"Email verification failed: {ex.Message}"));
        }
    }
}