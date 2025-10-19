using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Sales.Commands.ClearCart.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Sales.ClearCart.V1;

public class ClearCartEndpointV1 : Endpoint<ClearCartRequestV1, ApiResponse<string>>
{
    private readonly IMediator _mediator;

    public ClearCartEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/cart/clear");
        AllowAnonymous();
        Description(b => b
            .WithName("ClearCart")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
            .WithTags("Shopping Cart"));
    }

    public override async Task HandleAsync(ClearCartRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<string>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map the request to command
        var command = new ClearCartCommandV1
        {
            CartId = req.CartId
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
            "Cart successfully cleared",
            "Cart cleared successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}