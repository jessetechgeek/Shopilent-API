using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Catalog.Queries.GetVariant.V1;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Products.GetVariant.V1;

public class GetVariantEndpointV1 : EndpointWithoutRequest<ApiResponse<ProductVariantDto>>
{
    private readonly IMediator _mediator;

    public GetVariantEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/variants/{id}");
        AllowAnonymous(); 
        Description(b => b
            .WithName("GetVariantById")
            .Produces<ApiResponse<ProductVariantDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ProductVariantDto>>(StatusCodes.Status404NotFound)
            .WithTags("Products"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get the variant ID from the route
        var variantId = Route<Guid>("id");

        // Create query
        var query = new GetVariantQueryV1 { Id = variantId };

        // Send query to mediator
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<ProductVariantDto>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<ProductVariantDto>.Success(
            result.Value,
            "Variant retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}