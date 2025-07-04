using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Identity.Queries.GetCurrentUserProfile.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Identity.DTOs;

namespace Shopilent.API.Endpoints.Users.GetCurrentUserProfile.V1;

public class GetCurrentUserProfileEndpointV1 : EndpointWithoutRequest<ApiResponse<UserDetailDto>>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public GetCurrentUserProfileEndpointV1(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    public override void Configure()
    {
        Get("v1/users/me");
        Description(b => b
            .WithName("GetCurrentUserProfile")
            .Produces<ApiResponse<UserDetailDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UserDetailDto>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<UserDetailDto>>(StatusCodes.Status404NotFound)
            .WithTags("Users"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Create query
        var query = new GetCurrentUserProfileQueryV1
        {
            UserId = _currentUserContext.UserId.Value
        };

        // Send query to mediator
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
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
            "User profile retrieved successfully");

        await SendAsync(response, response.StatusCode, ct);
    }
}