using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Identity.Commands.ChangePassword.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Users.ChangePassword.V1;

public class ChangePasswordEndpointV1 : Endpoint<ChangePasswordRequestV1, ApiResponse<string>>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public ChangePasswordEndpointV1(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    public override void Configure()
    {
        Put("v1/users/change-password");
        Description(b => b
            .WithName("ChangePassword")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<string>>(StatusCodes.Status401Unauthorized)
            .WithTags("Users"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(ChangePasswordRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<string>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        if (_currentUserContext.UserId == null)
        {
            var unauthorizedResponse = ApiResponse<string>.Failure(
                "User not authenticated",
                StatusCodes.Status401Unauthorized);
            await SendAsync(unauthorizedResponse, unauthorizedResponse.StatusCode, ct);
            return;
        }

        // Map the request to command
        var command = new ChangePasswordCommandV1
        {
            UserId = _currentUserContext.UserId.Value,
            CurrentPassword = req.CurrentPassword,
            NewPassword = req.NewPassword,
            ConfirmPassword = req.ConfirmPassword
        };

        // Send the command to the handler
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<string>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<string>.Success(
            "Password changed successfully",
            "Password changed successfully");

        await SendAsync(response, response.StatusCode, ct);
    }
}