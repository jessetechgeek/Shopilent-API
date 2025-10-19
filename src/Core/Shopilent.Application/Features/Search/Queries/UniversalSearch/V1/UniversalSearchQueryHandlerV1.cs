using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Search;

using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Search.Queries.UniversalSearch.V1;

internal sealed class UniversalSearchQueryHandlerV1 : IQueryHandler<UniversalSearchQueryV1, SearchResponse<ProductSearchResultDto>>
{
    private readonly ISearchService _searchService;
    private readonly ILogger<UniversalSearchQueryHandlerV1> _logger;

    public UniversalSearchQueryHandlerV1(
        ISearchService searchService,
        ILogger<UniversalSearchQueryHandlerV1> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<Result<SearchResponse<ProductSearchResultDto>>> Handle(
        UniversalSearchQueryV1 request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing universal search with query: {Query}", request.Query);

            var searchRequest = new SearchRequest
            {
                Query = request.Query,
                CategorySlugs = request.CategorySlugs,
                AttributeFilters = request.AttributeFilters,
                PriceMin = request.PriceMin,
                PriceMax = request.PriceMax,
                InStockOnly = request.InStockOnly,
                ActiveOnly = request.ActiveOnly,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SortBy = request.SortBy,
                SortDescending = request.SortDescending
            };

            var result = await _searchService.SearchProductsAsync(searchRequest, cancellationToken);
            
            if (result.IsFailure)
            {
                _logger.LogError("Universal search failed: {Error}", result.Error.Message);
            }
            else
            {
                _logger.LogDebug("Universal search completed successfully. Found {Count} results", 
                    result.Value.TotalCount);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during universal search");
            return Result.Failure<SearchResponse<ProductSearchResultDto>>(
                Domain.Common.Errors.Error.Failure("Search.UnexpectedError", "An unexpected error occurred during search"));
        }
    }
}