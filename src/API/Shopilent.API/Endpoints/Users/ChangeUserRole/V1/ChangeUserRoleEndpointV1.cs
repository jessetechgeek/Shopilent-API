using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Identity.Commands.ChangeUserRole.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Users.ChangeUserRole.V1;

public class ChangeUserRoleEndpointV1 : Endpoint<ChangeUserRoleRequestV1, ApiResponse<string>>
{
    private readonly IMediator _mediator;

    public ChangeUserRoleEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("v1/users/{id}/role");
        Description(b => b
            .WithName("ChangeUserRole")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<string>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<string>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
            .WithTags("Users"));
        Policies(nameof(AuthorizationPolicy.RequireAdmin));
    }

    public override async Task HandleAsync(ChangeUserRoleRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<string>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Get user ID from route
        var userId = Route<Guid>("id");

        // Create command
        var command = new ChangeUserRoleCommandV1
        {
            UserId = userId,
            NewRole = req.Role
        };

        // Send command to mediator
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
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
            result.Value,
            "User role changed successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}