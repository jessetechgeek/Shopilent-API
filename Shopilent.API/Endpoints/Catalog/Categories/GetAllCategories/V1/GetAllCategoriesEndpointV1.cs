using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Catalog.Queries.GetAllCategories.V1;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.API.Endpoints.Catalog.Categories.GetAllCategories.V1;

public class GetAllCategoriesEndpointV1 : EndpointWithoutRequest<ApiResponse<IReadOnlyList<CategoryDto>>>
{
    private readonly IMediator _mediator;

    public GetAllCategoriesEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/categories/all");
        AllowAnonymous();
        Description(b => b
            .WithName("GetAllCategories")
            .Produces<ApiResponse<IReadOnlyList<CategoryDto>>>(StatusCodes.Status200OK)
            .WithTags("Categories"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Create query
        var query = new GetAllCategoriesQueryV1();

        // Send query to mediator
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<IReadOnlyList<CategoryDto>>.Failure(
                result.Error.Message,
                StatusCodes.Status500InternalServerError);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<IReadOnlyList<CategoryDto>>.Success(
            result.Value,
            "Categories retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}