using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Features.Identity.Commands.Login.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Application.Features.Identity.Commands.Register.V1;

internal sealed class RegisterCommandHandlerV1 : ICommandHandler<RegisterCommandV1, RegisterResponseV1>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<RegisterCommandHandlerV1> _logger;

    public RegisterCommandHandlerV1(IAuthenticationService authenticationService,
        ILogger<RegisterCommandHandlerV1> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result<RegisterResponseV1>> Handle(RegisterCommandV1 request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Create email value object
            var emailResult = Email.Create(request.Email);
            if (emailResult.IsFailure)
                return Result.Failure<RegisterResponseV1>(emailResult.Error);

            // Register the user
            var result = await _authenticationService.RegisterAsync(
                emailResult.Value,
                request.Password,
                request.FirstName,
                request.LastName,
                request.Phone,
                request.IpAddress,
                request.UserAgent,
                cancellationToken);
            if (result.IsFailure)
                return Result.Failure<RegisterResponseV1>(result.Error);

            var response = new RegisterResponseV1
            {
                User = result.Value.User,
                AccessToken = result.Value.AccessToken,
                RefreshToken = result.Value.RefreshToken
            };
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);

            return Result.Failure<RegisterResponseV1>(
                Error.Validation(
                    code: "Registration.Failed",
                    message: $"Registration failed: {ex.Message}"));
        }
    }
}