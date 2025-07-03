using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Commands.DeleteAttribute.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Attributes.DeleteAttribute.V1;

public class DeleteAttributeEndpointV1 : EndpointWithoutRequest
{
    private readonly IMediator _mediator;

    public DeleteAttributeEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("v1/attributes/{id}");
        Description(b => b
            .WithName("DeleteAttribute")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<string>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<string>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<string>>(StatusCodes.Status409Conflict)
            .WithTags("Attributes"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get attribute ID from route
        var attributeId = Route<Guid>("id");

        // Create command
        var command = new DeleteAttributeCommandV1 { Id = attributeId };

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
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<string>.Success(
            "Attribute deleted successfully",
            "Attribute deleted successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}