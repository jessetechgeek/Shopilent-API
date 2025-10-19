using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Payments.Commands.SetDefaultPaymentMethod.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Payments.SetDefaultPaymentMethod.V1;

public class SetDefaultPaymentMethodEndpointV1 : EndpointWithoutRequest<ApiResponse<SetDefaultPaymentMethodResponseV1>>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public SetDefaultPaymentMethodEndpointV1(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    public override void Configure()
    {
        Put("v1/payment-methods/{id}/default");
        Description(b => b
            .WithName("SetDefaultPaymentMethod")
            .Produces<ApiResponse<SetDefaultPaymentMethodResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SetDefaultPaymentMethodResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<SetDefaultPaymentMethodResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<SetDefaultPaymentMethodResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Payment Methods"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get the payment method ID from the route
        var paymentMethodId = Route<Guid>("id");

        // Get current user ID
        var currentUserId = _currentUserContext.UserId;
        if (!currentUserId.HasValue)
        {
            var unauthorizedResponse = ApiResponse<SetDefaultPaymentMethodResponseV1>.Failure(
                "User not authenticated",
                StatusCodes.Status401Unauthorized);

            await SendAsync(unauthorizedResponse, unauthorizedResponse.StatusCode, ct);
            return;
        }

        // Create command
        var command = new SetDefaultPaymentMethodCommandV1
        {
            PaymentMethodId = paymentMethodId,
            UserId = currentUserId.Value
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

            var errorResponse = ApiResponse<SetDefaultPaymentMethodResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<SetDefaultPaymentMethodResponseV1>.Success(
            result.Value,
            "Payment method set as default successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}