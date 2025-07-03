using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Catalog.Queries.GetCategory.V1;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Categories.GetCategory.V1;

public class GetCategoryEndpointV1 : EndpointWithoutRequest<ApiResponse<CategoryDto>>
{
    private readonly IMediator _mediator;

    public GetCategoryEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/categories/{id}");
        AllowAnonymous();
        Description(b => b
            .WithName("GetCategoryById")
            .Produces<ApiResponse<CategoryDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CategoryDto>>(StatusCodes.Status404NotFound)
            .WithTags("Categories"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get the ID from the route
        var id = Route<Guid>("id");

        // Create query
        var query = new GetCategoryQueryV1 { Id = id };

        // Send query to mediator
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<CategoryDto>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<CategoryDto>.Success(
            result.Value,
            "Category retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}