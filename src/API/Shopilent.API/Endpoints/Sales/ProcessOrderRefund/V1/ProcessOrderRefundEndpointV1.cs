using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Sales.Commands.ProcessOrderRefund.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Sales.ProcessOrderRefund.V1;

public class
    ProcessOrderRefundEndpointV1 : Endpoint<ProcessOrderRefundRequestV1, ApiResponse<ProcessOrderRefundResponseV1>>
{
    private readonly IMediator _mediator;

    public ProcessOrderRefundEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/orders/{id}/refund");
        Description(b => b
            .WithName("ProcessOrderRefund")
            .Produces<ApiResponse<ProcessOrderRefundResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ProcessOrderRefundResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<ProcessOrderRefundResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<ProcessOrderRefundResponseV1>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<ProcessOrderRefundResponseV1>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<ProcessOrderRefundResponseV1>>(StatusCodes.Status409Conflict)
            .WithTags("Orders"));
        Policies(nameof(AuthorizationPolicy.RequireAdmin));
    }

    public override async Task HandleAsync(ProcessOrderRefundRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<ProcessOrderRefundResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Get order ID from route
        var orderId = Route<Guid>("id");

        // Create command
        var command = new ProcessOrderRefundCommandV1
        {
            OrderId = orderId,
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
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<ProcessOrderRefundResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<ProcessOrderRefundResponseV1>.Success(
            result.Value,
            $"Full refund processed successfully for order {orderId}");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}