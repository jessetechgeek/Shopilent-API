using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Common.Constants;
using Shopilent.Application.Features.Catalog.Commands.CreateAttribute.V1;
using Shopilent.Domain.Catalog.Enums;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;

public class CreateAttributeEndpointV1 : Endpoint<CreateAttributeRequestV1, ApiResponse<CreateAttributeResponseV1>>
{
    private readonly IMediator _mediator;

    public CreateAttributeEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("v1/attributes");
        Description(b => b
            .WithName("CreateAttribute")
            .Produces<ApiResponse<CreateAttributeResponseV1>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<CreateAttributeResponseV1>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<CreateAttributeResponseV1>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<CreateAttributeResponseV1>>(StatusCodes.Status403Forbidden)
            .WithTags("Attributes"));
        Policies(nameof(AuthorizationPolicy.RequireAdminOrManager));
    }

    public override async Task HandleAsync(CreateAttributeRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<CreateAttributeResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        if (!Enum.TryParse<AttributeType>(req.Type, true, out var attributeType))
        {
            var errorResponse = ApiResponse<CreateAttributeResponseV1>.Failure(
                $"Invalid attribute type: {req.Type}. Valid values are: Text, Number, Boolean, Select, Color, Date, Dimensions, Weight.",
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }
        
        // Map request to command
        var command = new CreateAttributeCommandV1
        {
            Name = req.Name,
            DisplayName = req.DisplayName,
            // Type = req.Type,
            Type = attributeType, // Use the parsed enum value
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

            var errorResponse = ApiResponse<CreateAttributeResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map command result to response
        var response = new CreateAttributeResponseV1
        {
            Id = result.Value.Id,
            Name = result.Value.Name,
            DisplayName = result.Value.DisplayName,
            Type = result.Value.Type,
            Filterable = result.Value.Filterable,
            Searchable = result.Value.Searchable,
            IsVariant = result.Value.IsVariant,
            Configuration = result.Value.Configuration,
            CreatedAt = result.Value.CreatedAt
        };

        // Return successful response
        var apiResponse = ApiResponse<CreateAttributeResponseV1>.Success(
            response,
            "Attribute created successfully");

        await SendCreatedAtAsync("GetAttributeById", new { id = response.Id }, apiResponse, cancellation: ct);
    }
}