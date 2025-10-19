using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Shipping.Commands.CreateAddress.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Shipping.CreateAddress.V1;

public class CreateAddressEndpointV1 : Endpoint<CreateAddressRequestV1, ApiResponse<CreateAddressResponseV1>>
{
    private readonly IMediator _mediator;

    public CreateAddressEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/addresses");
        Description(b => b
            .WithName("CreateAddress")
            .Produces<ApiResponse<CreateAddressResponseV1>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<CreateAddressResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<CreateAddressResponseV1>>(StatusCodes.Status401Unauthorized)
            .WithTags("Addresses"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(CreateAddressRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<CreateAddressResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map request to command
        var command = new CreateAddressCommandV1
        {
            AddressLine1 = req.AddressLine1,
            AddressLine2 = req.AddressLine2,
            City = req.City,
            State = req.State,
            PostalCode = req.PostalCode,
            Country = req.Country,
            Phone = req.Phone,
            AddressType = req.AddressType,
            IsDefault = req.IsDefault
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

            var errorResponse = ApiResponse<CreateAddressResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<CreateAddressResponseV1>.Success(
            result.Value,
            "Address created successfully");

        await SendAsync(response, StatusCodes.Status201Created, ct);
    }
}