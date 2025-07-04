using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Sales.Queries.GetCart.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Sales.DTOs;

namespace Shopilent.API.Endpoints.Sales.GetCart.V1;

public class GetCartEndpointV1 : Endpoint<GetCartRequestV1, ApiResponse<CartDto?>>
{
    private readonly IMediator _mediator;

    public GetCartEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/cart");
        AllowAnonymous(); // Allow both authenticated and anonymous users
        Description(b => b
            .WithName("GetCart")
            .Produces<ApiResponse<CartDto?>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CartDto?>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<CartDto?>>(StatusCodes.Status500InternalServerError)
            .WithTags("Shopping Cart"));
    }

    public override async Task HandleAsync(GetCartRequestV1 req, CancellationToken ct)
    {
        // Map the request to query
        var query = new GetCartQueryV1
        {
            CartId = req.CartId
        };

        // Send the query to the handler
        var result = await _mediator.Send(query, ct);

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

            var errorResponse = ApiResponse<CartDto?>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var cart = result.Value;

        // Return success response (cart may be null if user has no cart)
        var response = cart != null
            ? ApiResponse<CartDto?>.Success(cart, "Cart retrieved successfully")
            : ApiResponse<CartDto?>.Success(null, "No cart found");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}