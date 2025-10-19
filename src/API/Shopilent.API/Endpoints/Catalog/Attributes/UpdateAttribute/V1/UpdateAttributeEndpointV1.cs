using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Commands.UpdateAttribute.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Attributes.UpdateAttribute.V1;

public class UpdateAttributeEndpointV1 : Endpoint<UpdateAttributeRequestV1, ApiResponse<UpdateAttributeResponseV1>>
{
    private readonly IMediator _mediator;

    public UpdateAttributeEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("v1/attributes/{id}");
        Description(b => b
            .WithName("UpdateAttribute")
            .Produces<ApiResponse<UpdateAttributeResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdateAttributeResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<UpdateAttributeResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<UpdateAttributeResponseV1>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<UpdateAttributeResponseV1>>(StatusCodes.Status404NotFound)
            .WithTags("Attributes"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(UpdateAttributeRequestV1 req, CancellationToken ct)
    {
        // Get ID from route
        var id = Route<Guid>("id");

        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<UpdateAttributeResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map request to command
        var command = new UpdateAttributeCommandV1
        {
            Id = id,
            DisplayName = req.DisplayName,
            Filterable = req.Filterable,
            Searchable = req.Searchable,
            IsVariant = req.IsVariant,
            Configuration = req.Configuration
        };

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

            var errorResponse = ApiResponse<UpdateAttributeResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map command result to response
        var response = new UpdateAttributeResponseV1
        {
            Id = result.Value.Id,
            Name = result.Value.Name,
            DisplayName = result.Value.DisplayName,
            Type = result.Value.Type,
            Filterable = result.Value.Filterable,
            Searchable = result.Value.Searchable,
            IsVariant = result.Value.IsVariant,
            Configuration = result.Value.Configuration,
            UpdatedAt = result.Value.UpdatedAt
        };

        // Return successful response
        var apiResponse = ApiResponse<UpdateAttributeResponseV1>.Success(
            response,
            "Attribute updated successfully");

        await SendAsync(apiResponse, StatusCodes.Status200OK, ct);
    }
}