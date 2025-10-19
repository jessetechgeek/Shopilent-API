using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Sales.Commands.CancelOrder.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Sales.CancelOrder.V1;

public class CancelOrderEndpointV1 : Endpoint<CancelOrderRequestV1, ApiResponse<CancelOrderResponseV1>>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public CancelOrderEndpointV1(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    public override void Configure()
    {
        Post("v1/orders/{id}/cancel");
        Description(b => b
            .WithName("CancelOrder")
            .Produces<ApiResponse<CancelOrderResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CancelOrderResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<CancelOrderResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<CancelOrderResponseV1>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<CancelOrderResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Orders"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(CancelOrderRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<CancelOrderResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Get the order ID from the route
        var orderId = Route<Guid>("id");

        // Create command with current user context
        var command = new CancelOrderCommandV1
        {
            OrderId = orderId,
            Reason = req.Reason,
            CurrentUserId = _currentUserContext.UserId,
            IsAdmin = _currentUserContext.IsInRole("Admin"),
            IsManager = _currentUserContext.IsInRole("Manager")
        };

        // Send command to mediator
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<CancelOrderResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<CancelOrderResponseV1>.Success(
            result.Value,
            "Order cancelled successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}