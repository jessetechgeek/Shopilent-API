using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Shipping.Commands.UpdateAddress.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Shipping.UpdateAddress.V1;

public class UpdateAddressEndpointV1 : Endpoint<UpdateAddressRequestV1, ApiResponse<UpdateAddressResponseV1>>
{
    private readonly IMediator _mediator;

    public UpdateAddressEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("v1/addresses/{id}");
        Description(b => b
            .WithName("UpdateAddress")
            .Produces<ApiResponse<UpdateAddressResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdateAddressResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<UpdateAddressResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<UpdateAddressResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Addresses"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(UpdateAddressRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<UpdateAddressResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Get ID from route
        var id = Route<Guid>("id");

        // Create command
        var command = new UpdateAddressCommandV1
        {
            Id = id,
            AddressLine1 = req.AddressLine1,
            AddressLine2 = req.AddressLine2,
            City = req.City,
            State = req.State,
            PostalCode = req.PostalCode,
            Country = req.Country,
            Phone = req.Phone,
            AddressType = req.AddressType
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

            var errorResponse = ApiResponse<UpdateAddressResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<UpdateAddressResponseV1>.Success(
            result.Value,
            "Address updated successfully");

        await SendAsync(response, response.StatusCode, ct);
    }
}