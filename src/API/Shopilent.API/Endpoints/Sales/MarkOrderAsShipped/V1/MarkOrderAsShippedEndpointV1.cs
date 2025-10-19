using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Sales.Commands.MarkOrderAsShipped.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Sales.MarkOrderAsShipped.V1;

public class MarkOrderAsShippedEndpointV1 : Endpoint<MarkOrderAsShippedRequestV1, ApiResponse<string>>
{
    private readonly IMediator _mediator;

    public MarkOrderAsShippedEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("v1/orders/{id}/shipped");
        Description(b => b
            .WithName("MarkOrderAsShipped")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<string>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<string>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
            .WithTags("Orders"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(MarkOrderAsShippedRequestV1 req, CancellationToken ct)
    {
        // Get order ID from route
        var orderId = Route<Guid>("id");

        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<string>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Create command
        var command = new MarkOrderAsShippedCommandV1
        {
            OrderId = orderId,
            TrackingNumber = req.TrackingNumber
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

            var errorResponse = ApiResponse<string>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<string>.Success(
            "Order marked as shipped successfully",
            !string.IsNullOrEmpty(req.TrackingNumber) 
                ? $"Order marked as shipped with tracking number: {req.TrackingNumber}" 
                : "Order marked as shipped successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}