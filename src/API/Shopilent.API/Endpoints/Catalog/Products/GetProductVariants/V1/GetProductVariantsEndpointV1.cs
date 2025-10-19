using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Catalog.Queries.GetProductVariants.V1;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Products.GetProductVariants.V1;

public class GetProductVariantsEndpointV1 : EndpointWithoutRequest<ApiResponse<IReadOnlyList<ProductVariantDto>>>
{
    private readonly IMediator _mediator;

    public GetProductVariantsEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/products/{id}/variants2");
        AllowAnonymous();
        Description(b => b
            .WithName("GetProductVariants")
            .Produces<ApiResponse<IReadOnlyList<ProductVariantDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<IReadOnlyList<ProductVariantDto>>>(StatusCodes.Status404NotFound)
            .WithTags("Products"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get the product ID from the route
        var productId = Route<Guid>("id");

        // Create query
        var query = new GetProductVariantsQueryV1 { ProductId = productId };

        // Send query to mediator
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<IReadOnlyList<ProductVariantDto>>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<IReadOnlyList<ProductVariantDto>>.Success(
            result.Value,
            "Product variants retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}