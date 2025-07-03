using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Application.Features.Identity.Commands.Login.V1;

internal sealed class
    LoginCommandHandlerV1 : ICommandHandler<LoginCommandV1,
    LoginResponseV1>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<LoginCommandHandlerV1> _logger;

    public LoginCommandHandlerV1(IAuthenticationService authenticationService, ILogger<LoginCommandHandlerV1> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result<LoginResponseV1>> Handle(LoginCommandV1 request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Create email value object
            var emailResult = Email.Create(request.Email);
            if (emailResult.IsFailure)
                return Result.Failure<LoginResponseV1>(emailResult.Error);

            // Authenticate the user
            var result = await _authenticationService.LoginAsync(
                emailResult.Value,
                request.Password,
                request.IpAddress,
                request.UserAgent,
                cancellationToken);

            if (result.IsFailure)
                return Result.Failure<LoginResponseV1>(result.Error);

            var loginResponse = new LoginResponseV1
            {
                User = result.Value.User,
                AccessToken = result.Value.AccessToken,
                RefreshToken = result.Value.RefreshToken
            };
            return Result.Success<LoginResponseV1>(loginResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return Result.Failure<LoginResponseV1>(
                Error.Validation(
                    code: "Login.Failed",
                    message: $"Login failed: {ex.Message}"));
        }
    }
}