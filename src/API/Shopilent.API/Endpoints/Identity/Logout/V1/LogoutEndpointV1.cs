using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Identity.Commands.Logout.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Identity.Logout.V1;

public class LogoutEndpointV1 : Endpoint<LogoutRequestV1, ApiResponse<string>>
{
    private readonly IMediator _mediator;

    public LogoutEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/auth/logout");
        Description(b => b
            .WithName("Logout")
            .Produces<ApiResponse<String>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<String>>(StatusCodes.Status401Unauthorized)
            .WithTags("Identity"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(LogoutRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<string>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }
        // Map the request to command
        var command = new LogoutCommandV1()
        {
            RefreshToken = req.RefreshToken,
            Reason = req.Reason
        };

        // Send the command to the handler
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.NotFound when result.Error.Code == "RefreshToken.NotFound" => StatusCodes.Status401Unauthorized, // Only treat specific refresh token not found as auth failure
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };

            var errorResponse = new ApiResponse<String>
            {
                Succeeded = false,
                Message = result.Error.Message,
                StatusCode = statusCode,
                Errors = new[] { result.Error.Message }
            };

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Handle successful logout
        var loginResponse = result.IsSuccess;
        var response = new ApiResponse<String>
        {
            Succeeded = true,
            Message = "Logout successful",
            StatusCode = StatusCodes.Status200OK,
            Data = "Logout successful"
        };

        await SendAsync(response, response.StatusCode, ct);
    }
}