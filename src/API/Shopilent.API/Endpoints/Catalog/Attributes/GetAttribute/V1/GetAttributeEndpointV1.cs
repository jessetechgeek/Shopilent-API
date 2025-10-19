using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Catalog.Queries.GetAttribute.V1;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Attributes.GetAttribute.V1;

public class GetAttributeEndpointV1 : EndpointWithoutRequest<ApiResponse<AttributeDto>>
{
    private readonly IMediator _mediator;

    public GetAttributeEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/attributes/{id}");
        AllowAnonymous();
        Description(b => b
            .WithName("GetAttributeById")
            .Produces<ApiResponse<AttributeDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<AttributeDto>>(StatusCodes.Status404NotFound)
            .WithTags("Attributes"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get the ID from the route
        var id = Route<Guid>("id");

        // Create query
        var query = new GetAttributeQueryV1 { Id = id };

        // Send query to mediator
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<AttributeDto>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<AttributeDto>.Success(
            result.Value,
            "Attribute retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}