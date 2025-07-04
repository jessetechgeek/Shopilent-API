using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Sales.Commands.UpdateCartItemQuantity.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Sales.UpdateCartItemQuantity.V1;

public class UpdateCartItemQuantityEndpointV1 : Endpoint<UpdateCartItemQuantityRequestV1, ApiResponse<UpdateCartItemQuantityResponseV1>>
{
    private readonly IMediator _mediator;

    public UpdateCartItemQuantityEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("v1/cart/items/{id}");
        AllowAnonymous();
        Description(b => b
            .WithName("UpdateCartItemQuantity")
            .Produces<ApiResponse<UpdateCartItemQuantityResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdateCartItemQuantityResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<UpdateCartItemQuantityResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Shopping Cart"));
    }

    public override async Task HandleAsync(UpdateCartItemQuantityRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<UpdateCartItemQuantityResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Get the cart item ID from the route
        var cartItemId = Route<Guid>("id");

        // Create command
        var command = new UpdateCartItemQuantityCommandV1
        {
            CartItemId = cartItemId,
            Quantity = req.Quantity
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
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<UpdateCartItemQuantityResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<UpdateCartItemQuantityResponseV1>.Success(
            result.Value,
            "Cart item quantity updated successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}