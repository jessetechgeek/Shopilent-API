using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Identity.Queries.GetUser.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Identity.DTOs;

namespace Shopilent.API.Endpoints.Users.GetUser.V1;

public class GetUserEndpointV1 : EndpointWithoutRequest<ApiResponse<UserDetailDto>>
{
    private readonly IMediator _mediator;

    public GetUserEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/users/{id}");
        Description(b => b
            .WithName("GetUserById")
            .Produces<ApiResponse<UserDetailDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UserDetailDto>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<UserDetailDto>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<UserDetailDto>>(StatusCodes.Status403Forbidden)
            .WithTags("Users"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get the ID from the route
        var id = Route<Guid>("id");

        // Create query
        var query = new GetUserQueryV1 { Id = id };

        // Send query to mediator
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<UserDetailDto>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<UserDetailDto>.Success(
            result.Value,
            "User retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}