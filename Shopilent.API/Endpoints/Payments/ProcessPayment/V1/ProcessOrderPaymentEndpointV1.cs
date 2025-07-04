using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Payments.Commands.ProcessOrderPayment.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Payments.ProcessPayment.V1;

public class
    ProcessOrderPaymentEndpointV1 : Endpoint<ProcessOrderPaymentRequestV1, ApiResponse<ProcessOrderPaymentResponseV1>>
{
    private readonly IMediator _mediator;

    public ProcessOrderPaymentEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/orders/{id}/payments");
        Description(b => b
            .WithName("ProcessOrderPayment")
            .Produces<ApiResponse<ProcessOrderPaymentResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ProcessOrderPaymentResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<ProcessOrderPaymentResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<ProcessOrderPaymentResponseV1>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<ProcessOrderPaymentResponseV1>>(StatusCodes.Status500InternalServerError)
            .WithTags("Payments"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(ProcessOrderPaymentRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<ProcessOrderPaymentResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var orderId = Route<Guid>("id");

        var command = new ProcessOrderPaymentCommandV1
        {
            OrderId = orderId,
            PaymentMethodId = req.PaymentMethodId,
            MethodType = req.MethodType,
            Provider = req.Provider,
            PaymentMethodToken = req.PaymentMethodToken,
            ExternalReference = req.ExternalReference,
            Metadata = req.Metadata
        };

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var response = result.Error.Type switch
            {
                ErrorType.NotFound => ApiResponse<ProcessOrderPaymentResponseV1>.Failure(
                    result.Error.Message, StatusCodes.Status404NotFound),
                ErrorType.Validation => ApiResponse<ProcessOrderPaymentResponseV1>.Failure(
                    result.Error.Message, StatusCodes.Status400BadRequest),
                ErrorType.Unauthorized => ApiResponse<ProcessOrderPaymentResponseV1>.Failure(
                    result.Error.Message, StatusCodes.Status401Unauthorized),
                _ => ApiResponse<ProcessOrderPaymentResponseV1>.Failure(
                    result.Error.Message, StatusCodes.Status500InternalServerError)
            };

            await SendAsync(response, response.StatusCode, ct);
            return;
        }

        var successResponse = ApiResponse<ProcessOrderPaymentResponseV1>.Success(
            result.Value,
            "Payment processed successfully");

        await SendAsync(successResponse, StatusCodes.Status200OK, ct);
    }
}