using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Catalog.Queries.GetRootCategories.V1;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Categories.GetRootCategories.V1;

public class GetRootCategoriesEndpointV1 : EndpointWithoutRequest<ApiResponse<IReadOnlyList<CategoryDto>>>
{
    private readonly IMediator _mediator;

    public GetRootCategoriesEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/categories/root");
        AllowAnonymous();
        Description(b => b
            .WithName("GetRootCategories")
            .Produces<ApiResponse<IReadOnlyList<CategoryDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<IReadOnlyList<CategoryDto>>>(StatusCodes.Status500InternalServerError)
            .WithTags("Categories"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Create query
        var query = new GetRootCategoriesQueryV1();

        // Send query to mediator
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<IReadOnlyList<CategoryDto>>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Return successful response
        var response = ApiResponse<IReadOnlyList<CategoryDto>>.Success(
            result.Value,
            "Root categories retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}