using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Shipping.Queries.GetAddressById.V1;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Shipping.DTOs;

namespace Shopilent.API.Endpoints.Shipping.GetAddressById.V1;

public class GetAddressByIdEndpointV1 : Endpoint<GetAddressByIdRequestV1, ApiResponse<AddressDto>>
{
    private readonly IMediator _mediator;

    public GetAddressByIdEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/addresses/{id}");
        Description(b => b
            .WithName("GetAddressById")
            .Produces<ApiResponse<AddressDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<AddressDto>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<AddressDto>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<AddressDto>>(StatusCodes.Status404NotFound)
            .WithTags("Addresses"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(GetAddressByIdRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<AddressDto>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Create query
        var query = new GetAddressByIdQueryV1
        {
            AddressId = req.Id
        };

        // Send query to mediator
        var result = await _mediator.Send(query, ct);

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

            var errorResponse = ApiResponse<AddressDto>.Failure(
                new[] { result.Error.Message },
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<AddressDto>.Success(
            result.Value,
            "Address retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}