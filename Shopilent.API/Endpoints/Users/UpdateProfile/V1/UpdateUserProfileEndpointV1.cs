using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Identity.Commands.UpdateUserProfile.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Users.UpdateProfile.V1;

public class
    UpdateUserProfileEndpointV1 : Endpoint<UpdateUserProfileRequestV1, ApiResponse<UpdateUserProfileResponseV1>>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public UpdateUserProfileEndpointV1(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    public override void Configure()
    {
        Put("v1/users/me");
        Description(b => b
            .WithName("UpdateUserProfile")
            .Produces<ApiResponse<UpdateUserProfileResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdateUserProfileResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<UpdateUserProfileResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<UpdateUserProfileResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Users"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(UpdateUserProfileRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<UpdateUserProfileResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        if (_currentUserContext.UserId == null)
        {
            var unauthorizedResponse = ApiResponse<UpdateUserProfileResponseV1>.Failure(
                "User not authenticated",
                StatusCodes.Status401Unauthorized);
            await SendAsync(unauthorizedResponse, unauthorizedResponse.StatusCode, ct);
            return;
        }

        // Map the request to command
        var command = new UpdateUserProfileCommandV1
        {
            UserId = _currentUserContext.UserId.Value,
            FirstName = req.FirstName,
            LastName = req.LastName,
            MiddleName = req.MiddleName,
            Phone = req.Phone
        };

        // Send the command to the handler
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<UpdateUserProfileResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Handle successful update
        var response = ApiResponse<UpdateUserProfileResponseV1>.Success(
            result.Value,
            "User profile updated successfully");

        await SendAsync(response, response.StatusCode, ct);
    }
}