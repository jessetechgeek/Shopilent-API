using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Sales.Commands.CreateOrderFromCart.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Sales.CreateOrderFromCart.V1;

public class CreateOrderFromCartEndpointV1 : Endpoint<CreateOrderFromCartRequestV1, ApiResponse<CreateOrderFromCartResponseV1>>
{
    private readonly IMediator _mediator;

    public CreateOrderFromCartEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/orders");
        Description(b => b
            .WithName("CreateOrderFromCart")
            .Produces<ApiResponse<CreateOrderFromCartResponseV1>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<CreateOrderFromCartResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<CreateOrderFromCartResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<CreateOrderFromCartResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Orders"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(CreateOrderFromCartRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<CreateOrderFromCartResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map request to command
        var command = new CreateOrderFromCartCommandV1
        {
            CartId = req.CartId,
            ShippingAddressId = req.ShippingAddressId,
            BillingAddressId = req.BillingAddressId,
            ShippingMethod = req.ShippingMethod,
            Metadata = req.Metadata
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

            var errorResponse = ApiResponse<CreateOrderFromCartResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<CreateOrderFromCartResponseV1>.Success(
            result.Value,
            "Order created successfully");

        await SendAsync(response, StatusCodes.Status201Created, ct);
    }
}