using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Payments.Commands.DeletePaymentMethod.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Payments.DeletePaymentMethod.V1;

public class DeletePaymentMethodEndpointV1 : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public DeletePaymentMethodEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("v1/payment-methods/{id}");
        Description(b => b
            .WithName("DeletePaymentMethod")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<string>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<string>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<string>>(StatusCodes.Status409Conflict)
            .WithTags("Payment Methods"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get payment method ID from route
        var paymentMethodId = Route<Guid>("id");

        // Create command
        var command = new DeletePaymentMethodCommandV1 { Id = paymentMethodId };

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

            var errorResponse = ApiResponse<string>.Failure(
                new[] { result.Error.Message },
                statusCode);

            await SendAsync(errorResponse, statusCode, ct);
            return;
        }

        var successResponse = ApiResponse<string>.Success(
            "Payment method deleted successfully",
            "Payment method deleted successfully");

        await SendAsync(successResponse, StatusCodes.Status200OK, ct);
    }
}