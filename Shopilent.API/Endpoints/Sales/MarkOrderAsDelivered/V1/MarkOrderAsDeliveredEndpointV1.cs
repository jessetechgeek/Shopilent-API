using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Sales.Commands.MarkOrderAsDelivered.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Sales.MarkOrderAsDelivered.V1;

public class MarkOrderAsDeliveredEndpointV1 : EndpointWithoutRequest<ApiResponse<MarkOrderAsDeliveredResponseV1>>
{
    private readonly IMediator _mediator;

    public MarkOrderAsDeliveredEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("v1/orders/{id}/delivered");
        Description(b => b
            .WithName("MarkOrderAsDelivered")
            .Produces<ApiResponse<MarkOrderAsDeliveredResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<MarkOrderAsDeliveredResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<MarkOrderAsDeliveredResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<MarkOrderAsDeliveredResponseV1>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<MarkOrderAsDeliveredResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Orders"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get order ID from route
        var orderId = Route<Guid>("id");

        // Create command
        var command = new MarkOrderAsDeliveredCommandV1
        {
            OrderId = orderId
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

            var errorResponse = ApiResponse<MarkOrderAsDeliveredResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<MarkOrderAsDeliveredResponseV1>.Success(
            result.Value,
            "Order marked as delivered successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}