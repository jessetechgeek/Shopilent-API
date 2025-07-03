using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.Application.Features.Catalog.Queries.GetPaginatedCategories.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Categories.GetPaginatedCategories.V1;

public class GetPaginatedCategoriesEndpointV1 : 
    Endpoint<GetPaginatedCategoriesRequestV1, ApiResponse<GetPaginatedCategoriesResponseV1>>
{
    private readonly IMediator _mediator;

    public GetPaginatedCategoriesEndpointV1(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("v1/categories");
        AllowAnonymous();
        Description(b => b
            .WithName("GetPaginatedCategories")
            .Produces<ApiResponse<GetPaginatedCategoriesResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GetPaginatedCategoriesResponseV1>>(StatusCodes.Status400BadRequest)
            .WithTags("Categories"));
    }

    public override async Task HandleAsync(GetPaginatedCategoriesRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<GetPaginatedCategoriesResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Create query
        var query = new GetPaginatedCategoriesQueryV1
        {
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            SortColumn = req.SortColumn,
            SortDescending = req.SortDescending
        };

        // Send query to mediator
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<GetPaginatedCategoriesResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        // Map the paginated result to response model
        var paginatedResult = result.Value;
        var response = new GetPaginatedCategoriesResponseV1
        {
            Items = paginatedResult.Items,
            PageNumber = paginatedResult.PageNumber,
            PageSize = paginatedResult.PageSize,
            TotalCount = paginatedResult.TotalCount,
            TotalPages = paginatedResult.TotalPages,
            HasPreviousPage = paginatedResult.HasPreviousPage,
            HasNextPage = paginatedResult.HasNextPage
        };

        // Return successful response
        var apiResponse = ApiResponse<GetPaginatedCategoriesResponseV1>.Success(
            response,
            "Categories retrieved successfully");

        await SendAsync(apiResponse, StatusCodes.Status200OK, ct);
    }
}