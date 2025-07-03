using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Catalog.Queries.GetChildCategories.V1;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Categories.GetChildCategories.V1;

public class GetChildCategoriesEndpointV1 : EndpointWithoutRequest<ApiResponse<IReadOnlyList<CategoryDto>>>
{
    private readonly IMediator _mediator;

    public GetChildCategoriesEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/categories/{id}/children");
        AllowAnonymous();
        Description(b => b
            .WithName("GetChildCategories")
            .Produces<ApiResponse<IReadOnlyList<CategoryDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<IReadOnlyList<CategoryDto>>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<IReadOnlyList<CategoryDto>>>(StatusCodes.Status500InternalServerError)
            .WithTags("Categories"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get parent category ID from route
        var parentId = Route<Guid>("id");

        // Create query
        var query = new GetChildCategoriesQueryV1 { ParentId = parentId };

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
            $"Child categories for parent ID {parentId} retrieved successfully");

        await SendAsync(response, StatusCodes.Status200OK, ct);
    }
}