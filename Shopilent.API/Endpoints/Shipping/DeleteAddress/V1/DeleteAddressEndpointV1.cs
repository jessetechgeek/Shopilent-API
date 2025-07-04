using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Shipping.Commands.DeleteAddress.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Shipping.DeleteAddress.V1;

public class DeleteAddressEndpointV1 : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public DeleteAddressEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("v1/addresses/{id}");
        Description(b => b
            .WithName("DeleteAddress")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<string>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<string>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
            .WithTags("Addresses"));
        Policies(nameof(AuthorizationPolicy.RequireAuthenticated));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get address ID from route
        var addressId = Route<Guid>("id");

        // Create command
        var command = new DeleteAddressCommandV1 { Id = addressId };

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

            var errorResponse = ApiResponse<string>.Failure(
                new[] { result.Error.Message },
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Success response
        var successResponse = ApiResponse<string>.Success(
            "Address deleted successfully");

        await SendAsync(successResponse, successResponse.StatusCode, ct);
    }
}