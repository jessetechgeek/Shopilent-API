using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Sales.Commands.RemoveItemFromCart.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Sales.RemoveItemFromCart.V1;

public class RemoveItemFromCartEndpointV1 : Endpoint<RemoveItemFromCartRequestV1, ApiResponse<string>>
{
    private readonly IMediator _mediator;

    public RemoveItemFromCartEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("v1/cart/items/{id}");
        AllowAnonymous();
        Description(b => b
            .WithName("RemoveItemFromCart")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
            .WithTags("Shopping Cart"));
    }

    public override async Task HandleAsync(RemoveItemFromCartRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<string>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Get the cart item ID from the route
        var cartItemId = Route<Guid>("id");

        // Map the request to command
        var command = new RemoveItemFromCartCommandV1
        {
            ItemId = cartItemId
        };

        // Send the command to the handler
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<string>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var response = ApiResponse<string>.Success(
            "Item successfully removed from cart",
            "Item removed from cart");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}