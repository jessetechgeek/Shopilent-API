using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Sales.Commands.UpdateOrderStatus.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Sales.UpdateOrderStatus.V1;

public class
    UpdateOrderStatusEndpointV1 : Endpoint<UpdateOrderStatusRequestV1, ApiResponse<UpdateOrderStatusResponseV1>>
{
    private readonly IMediator _mediator;

    public UpdateOrderStatusEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("v1/orders/{id}/status");
        Description(b => b
            .WithName("UpdateOrderStatus")
            .Produces<ApiResponse<UpdateOrderStatusResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdateOrderStatusResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<UpdateOrderStatusResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<UpdateOrderStatusResponseV1>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<UpdateOrderStatusResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Orders"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(UpdateOrderStatusRequestV1 req, CancellationToken ct)
    {
        // Get ID from route
        var id = Route<Guid>("id");

        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<UpdateOrderStatusResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Create command
        var command = new UpdateOrderStatusCommandV1
        {
            Id = id,
            Status = req.Status,
            Reason = req.Reason
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

            var errorResponse = ApiResponse<UpdateOrderStatusResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map application response to API response
        var response = ApiResponse<UpdateOrderStatusResponseV1>.Success(
            new UpdateOrderStatusResponseV1
            {
                Id = result.Value.Id,
                Status = result.Value.Status,
                PaymentStatus = result.Value.PaymentStatus,
                UpdatedAt = result.Value.UpdatedAt
            },
            "Order status updated successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}