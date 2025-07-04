using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Shipping.Commands.SetAddressDefault.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Shipping.SetAddressDefault.V1;

public class
    SetAddressDefaultEndpointV1 : Endpoint<SetAddressDefaultRequestV1, ApiResponse<SetAddressDefaultResponseV1>>
{
    private readonly IMediator _mediator;

    public SetAddressDefaultEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("v1/addresses/{id}/default");
        Description(b => b
            .WithName("SetAddressDefault")
            .Produces<ApiResponse<SetAddressDefaultResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SetAddressDefaultResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<SetAddressDefaultResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<SetAddressDefaultResponseV1>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<SetAddressDefaultResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Addresses"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(SetAddressDefaultRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<SetAddressDefaultResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map the request to command
        var command = new SetAddressDefaultCommandV1
        {
            AddressId = req.Id
        };

        // Send the command to the handler
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<SetAddressDefaultResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, statusCode, ct);
            return;
        }

        // Map AddressDto to response
        var addressDto = result.Value;
        var response = new SetAddressDefaultResponseV1
        {
            Id = addressDto.Id,
            UserId = addressDto.UserId,
            AddressLine1 = addressDto.AddressLine1,
            AddressLine2 = addressDto.AddressLine2,
            City = addressDto.City,
            State = addressDto.State,
            PostalCode = addressDto.PostalCode,
            Country = addressDto.Country,
            Phone = addressDto.Phone,
            IsDefault = addressDto.IsDefault,
            AddressType = addressDto.AddressType,
            CreatedAt = addressDto.CreatedAt,
            UpdatedAt = addressDto.UpdatedAt
        };

        var successResponse = ApiResponse<SetAddressDefaultResponseV1>.Success(
            response,
            "Address has been successfully set as default");

        await SendOkAsync(successResponse, ct);
    }
}