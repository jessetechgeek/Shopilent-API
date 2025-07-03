using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Catalog.Queries.GetAllAttributes.V1;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.API.Endpoints.Catalog.Attributes.GetAllAttributes.V1;

public class GetAllAttributesEndpointV1 : EndpointWithoutRequest<ApiResponse<IReadOnlyList<AttributeDto>>>
{
    private readonly IMediator _mediator;

    public GetAllAttributesEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/attributes");
        AllowAnonymous();
        Description(b => b
            .WithName("GetAllAttributes")
            .Produces<ApiResponse<IReadOnlyList<AttributeDto>>>(StatusCodes.Status200OK)
            .WithTags("Attributes"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Create query
        var query = new GetAllAttributesQueryV1();

        // Send query to mediator
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<IReadOnlyList<AttributeDto>>.Failure(
                result.Error.Message,
                StatusCodes.Status500InternalServerError);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<IReadOnlyList<AttributeDto>>.Success(
            result.Value,
            "Attributes retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}