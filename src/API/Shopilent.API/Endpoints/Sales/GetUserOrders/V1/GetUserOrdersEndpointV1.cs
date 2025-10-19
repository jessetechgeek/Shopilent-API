using System.Security.Claims;
using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Sales.Queries.GetUserOrders.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Sales.DTOs;

namespace Shopilent.API.Endpoints.Sales.GetUserOrders.V1;

public class GetUserOrdersEndpointV1 : EndpointWithoutRequest<ApiResponse<IReadOnlyList<OrderDto>>>
{
    private readonly IMediator _mediator;

    public GetUserOrdersEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/orders/my-orders");
        Description(b => b
            .WithName("GetUserOrders")
            .Produces<ApiResponse<IReadOnlyList<OrderDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<IReadOnlyList<OrderDto>>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<IReadOnlyList<OrderDto>>>(StatusCodes.Status404NotFound)
            .WithTags("Orders"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get the current user's ID from the JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            var unauthorizedResponse = ApiResponse<IReadOnlyList<OrderDto>>.Failure(
                "Invalid or missing user identifier",
                StatusCodes.Status401Unauthorized);

            await SendAsync(unauthorizedResponse, unauthorizedResponse.StatusCode, ct);
            return;
        }

        // Create query
        var query = new GetUserOrdersQueryV1 { UserId = userId };

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

            var errorResponse = ApiResponse<IReadOnlyList<OrderDto>>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<IReadOnlyList<OrderDto>>.Success(
            result.Value,
            "User orders retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}