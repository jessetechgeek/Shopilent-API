using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Shipping.Queries.GetUserAddresses.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Shipping.GetUserAddresses.V1;

public class GetUserAddressesEndpointV1 : EndpointWithoutRequest<ApiResponse<GetUserAddressesResponseV1>>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public GetUserAddressesEndpointV1(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    public override void Configure()
    {
        Get("v1/addresses");
        Description(b => b
            .WithName("GetUserAddresses")
            .Produces<ApiResponse<GetUserAddressesResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GetUserAddressesResponseV1>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<GetUserAddressesResponseV1>>(StatusCodes.Status500InternalServerError)
            .WithTags("Addresses"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Check if user is authenticated
        if (_currentUserContext.UserId == null)
        {
            var unauthorizedResponse = ApiResponse<GetUserAddressesResponseV1>.Failure(
                "User not authenticated",
                StatusCodes.Status401Unauthorized);
            await SendAsync(unauthorizedResponse, unauthorizedResponse.StatusCode, ct);
            return;
        }

        var query = new GetUserAddressesQueryV1
        {
            UserId = _currentUserContext.UserId.Value
        };

        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<GetUserAddressesResponseV1>.Failure(
                new[] { result.Error.Message },
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var response = ApiResponse<GetUserAddressesResponseV1>.Success(
            new GetUserAddressesResponseV1
            {
                Addresses = result.Value
            },
            "User addresses retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}