using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Payments.Queries.GetUserPaymentMethods.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Payments.DTOs;

namespace Shopilent.API.Endpoints.Payments.GetUserPaymentMethods.V1;

public class GetUserPaymentMethodsEndpointV1 : EndpointWithoutRequest<ApiResponse<IReadOnlyList<PaymentMethodDto>>>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public GetUserPaymentMethodsEndpointV1(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    public override void Configure()
    {
        Get("v1/payment-methods");
        Description(b => b
            .WithName("GetUserPaymentMethods")
            .Produces<ApiResponse<IReadOnlyList<PaymentMethodDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<IReadOnlyList<PaymentMethodDto>>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<IReadOnlyList<PaymentMethodDto>>>(StatusCodes.Status500InternalServerError)
            .WithTags("Payment Methods"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get current user ID from the authentication context
        var currentUserId = _currentUserContext.UserId!.Value;

        // Create query
        var query = new GetUserPaymentMethodsQueryV1 { UserId = currentUserId };

        // Send query to mediator
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<IReadOnlyList<PaymentMethodDto>>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<IReadOnlyList<PaymentMethodDto>>.Success(
            result.Value,
            "Payment methods retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}