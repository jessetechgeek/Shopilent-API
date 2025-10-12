using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Search;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Catalog.Queries.GetPaginatedProducts.V1;

internal sealed class GetPaginatedProductsQueryHandlerV1 :
    IQueryHandler<GetPaginatedProductsQueryV1, SearchResponse<ProductSearchResultDto>>
{
    private readonly ISearchService _searchService;
    private readonly ILogger<GetPaginatedProductsQueryHandlerV1> _logger;

    public GetPaginatedProductsQueryHandlerV1(
        ISearchService searchService,
        ILogger<GetPaginatedProductsQueryHandlerV1> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<Result<SearchResponse<ProductSearchResultDto>>> Handle(
        GetPaginatedProductsQueryV1 request,
        CancellationToken cancellationToken)
    {
        try
        {
            var searchRequest = new SearchRequest
            {
                Query = request.SearchQuery ?? "",
                CategorySlugs = request.CategorySlugs,
                AttributeFilters = request.AttributeFilters,
                PriceMin = request.PriceMin,
                PriceMax = request.PriceMax,
                InStockOnly = request.InStockOnly,
                ActiveOnly = request.IsActiveOnly,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SortBy = MapSortColumn(request.SortColumn),
                SortDescending = request.SortDescending
            };

            var searchResult = await _searchService.SearchProductsAsync(searchRequest, cancellationToken);

            if (searchResult.IsFailure)
            {
                _logger.LogError("Product search failed: {Error}", searchResult.Error.Message);
                return searchResult;
            }

            _logger.LogInformation(
                "Retrieved products via search: Page {PageNumber}, Size {PageSize}, Total {TotalCount}",
                request.PageNumber, request.PageSize, searchResult.Value.TotalCount);

            return searchResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paginated products");

            return Result.Failure<SearchResponse<ProductSearchResultDto>>(
                Error.Failure(
                    code: "Products.GetPaginatedFailed",
                    message: $"Failed to retrieve paginated products: {ex.Message}"));
        }
    }


    private static string MapSortColumn(string userFriendlyColumn)
    {
        return userFriendlyColumn switch
        {
            "Name" => "name",
            "BasePrice" => "price",
            "CreatedAt" => "created",
            "UpdatedAt" => "updated",
            _ => "name"
        };
    }
}
