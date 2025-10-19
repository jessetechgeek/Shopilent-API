using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Payments.Queries.GetPaymentMethod.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Payments.DTOs;

namespace Shopilent.API.Endpoints.Payments.GetPaymentMethod.V1;

public class GetPaymentMethodEndpointV1 : EndpointWithoutRequest<ApiResponse<PaymentMethodDto>>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public GetPaymentMethodEndpointV1(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    public override void Configure()
    {
        Get("v1/payment-methods/{id}");
        Description(b => b
            .WithName("GetPaymentMethodById")
            .Produces<ApiResponse<PaymentMethodDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<PaymentMethodDto>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<PaymentMethodDto>>(StatusCodes.Status403Forbidden)
            .WithTags("Payment Methods"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));        
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get the ID from the route
        var id = Route<Guid>("id");
        var currentUserId = _currentUserContext.UserId!.Value;

        // Create query
        var query = new GetPaymentMethodQueryV1
        {
            Id = id,
            UserId = currentUserId
        };

        // Send query to mediator
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<PaymentMethodDto>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<PaymentMethodDto>.Success(
            result.Value,
            "Payment method retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}