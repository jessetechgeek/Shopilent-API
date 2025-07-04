using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Sales.Commands.ProcessOrderPartialRefund.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Sales.ProcessOrderPartialRefund.V1;

public class ProcessOrderPartialRefundEndpointV1
    : Endpoint<ProcessOrderPartialRefundRequestV1, ApiResponse<ProcessOrderPartialRefundResponseV1>>
{
    private readonly IMediator _mediator;

    public ProcessOrderPartialRefundEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/orders/{id}/partial-refund");
        Description(b => b
            .WithName("ProcessOrderPartialRefund")
            .Produces<ApiResponse<ProcessOrderPartialRefundResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ProcessOrderPartialRefundResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<ProcessOrderPartialRefundResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<ProcessOrderPartialRefundResponseV1>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<ProcessOrderPartialRefundResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Orders"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(ProcessOrderPartialRefundRequestV1 req, CancellationToken ct)
    {
        // Get order ID from route
        var orderId = Route<Guid>("id");

        // Validate request early
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<ProcessOrderPartialRefundResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map request to command
        var command = new ProcessOrderPartialRefundCommandV1
        {
            OrderId = orderId,
            Amount = req.Amount,
            Currency = req.Currency,
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
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<ProcessOrderPartialRefundResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<ProcessOrderPartialRefundResponseV1>.Success(
            result.Value,
            "Partial refund processed successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}