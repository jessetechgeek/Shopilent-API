using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Sales.Commands.AddItemToCart.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Sales.AddItemToCart.V1;

public class AddItemToCartEndpointV1 : Endpoint<AddItemToCartRequestV1, ApiResponse<AddItemToCartResponseV1>>
{
    private readonly IMediator _mediator;

    public AddItemToCartEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/cart/items");
        Description(b => b
            .WithName("AddItemToCart")
            .Produces<ApiResponse<AddItemToCartResponseV1>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<AddItemToCartResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<AddItemToCartResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<AddItemToCartResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Shopping Cart"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddItemToCartRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<AddItemToCartResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map request to command
        var command = new AddItemToCartCommandV1
        {
            CartId = req.CartId,
            ProductId = req.ProductId,
            VariantId = req.VariantId,
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

            var errorResponse = ApiResponse<AddItemToCartResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var apiResponse = ApiResponse<AddItemToCartResponseV1>.Success(
            result.Value,
            "Item added to cart successfully");

        await SendAsync(apiResponse, StatusCodes.Status201Created, ct);
    }
}