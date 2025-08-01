using FastEndpoints;
using MediatR;
using Shopilent.API.Common.Models;
using Shopilent.API.Common.Services;
using Shopilent.Application.Features.Catalog.Queries.GetPaginatedProducts.V1;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Endpoints.Catalog.Products.GetPaginatedProducts.V1;

public class GetPaginatedProductsEndpointV1 :
    Endpoint<GetPaginatedProductsRequestV1, ApiResponse<GetPaginatedProductsResponseV1>>
{
    private readonly IMediator _mediator;
    private readonly IFilterEncodingService _filterEncodingService;

    public GetPaginatedProductsEndpointV1(IMediator mediator, IFilterEncodingService filterEncodingService)
    {
        _mediator = mediator;
        _filterEncodingService = filterEncodingService;
    }

    public override void Configure()
    {
        Get("v1/products");
        AllowAnonymous(); 
        Description(b => b
            .WithName("GetPaginatedProducts")
            .Produces<ApiResponse<GetPaginatedProductsResponseV1>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GetPaginatedProductsResponseV1>>(StatusCodes.Status400BadRequest)
            .WithTags("Products"));
    }

    public override async Task HandleAsync(GetPaginatedProductsRequestV1 req, CancellationToken ct)
    {
        if (ValidationFailed)
        {
            var errorResponse = ApiResponse<GetPaginatedProductsResponseV1>.Failure(
                ValidationFailures.Select(f => f.ErrorMessage).ToArray(),
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var filtersResult = _filterEncodingService.DecodeFilters(req.FiltersBase64);
        if (filtersResult.IsFailure)
        {
            var errorResponse = ApiResponse<GetPaginatedProductsResponseV1>.Failure(
                filtersResult.Error.Message,
                StatusCodes.Status400BadRequest);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var filters = filtersResult.Value;

        var query = new GetPaginatedProductsQueryV1
        {
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize,
            SortColumn = filters.SortColumn,
            SortDescending = filters.SortDescending,
            CategoryId = filters.CategoryId,
            IsActiveOnly = filters.ActiveOnly,
            SearchQuery = filters.SearchQuery,
            AttributeFilters = filters.AttributeFilters,
            PriceMin = filters.PriceMin,
            PriceMax = filters.PriceMax,
            CategoryIds = filters.CategoryIds,
            InStockOnly = filters.InStockOnly
        };

        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorResponse = ApiResponse<GetPaginatedProductsResponseV1>.Failure(
                result.Error.Message,
                statusCode);

            await SendAsync(errorResponse, errorResponse.StatusCode, ct);
            return;
        }

        var paginatedResult = result.Value;
        var response = new GetPaginatedProductsResponseV1
        {
            Items = paginatedResult.Items,
            PageNumber = paginatedResult.PageNumber,
            PageSize = paginatedResult.PageSize,
            TotalCount = paginatedResult.TotalCount,
            TotalPages = paginatedResult.TotalPages,
            HasPreviousPage = paginatedResult.HasPreviousPage,
            HasNextPage = paginatedResult.HasNextPage
        };

        var apiResponse = ApiResponse<GetPaginatedProductsResponseV1>.Success(
            response,
            "Products retrieved successfully");

        await SendAsync(apiResponse, StatusCodes.Status200OK, ct);
    }

}