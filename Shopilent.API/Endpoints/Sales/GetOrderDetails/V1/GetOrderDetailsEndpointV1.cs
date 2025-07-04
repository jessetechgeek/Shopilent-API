using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Sales.Queries.GetOrderDetails.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Sales.DTOs;

namespace Shopilent.API.Endpoints.Sales.GetOrderDetails.V1;

public class GetOrderDetailsEndpointV1 : EndpointWithoutRequest<ApiResponse<OrderDetailDto>>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public GetOrderDetailsEndpointV1(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    public override void Configure()
    {
        Get("v1/orders/{id}");
        Description(b => b
            .WithName("GetOrderDetails")
            .Produces<ApiResponse<OrderDetailDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<OrderDetailDto>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<OrderDetailDto>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<OrderDetailDto>>(StatusCodes.Status403Forbidden)
            .WithTags("Orders"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get the order ID from the route
        var orderId = Route<Guid>("id");

        // Create query with current user context
        var query = new GetOrderDetailsQueryV1
        {
            OrderId = orderId,
            CurrentUserId = _currentUserContext.UserId,
            IsAdmin = _currentUserContext.IsInRole("Admin"),
            IsManager = _currentUserContext.IsInRole("Manager")
        };

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

            var errorResponse = ApiResponse<OrderDetailDto>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<OrderDetailDto>.Success(
            result.Value,
            "Order details retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}