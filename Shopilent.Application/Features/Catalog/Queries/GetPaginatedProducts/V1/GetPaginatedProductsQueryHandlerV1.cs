using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Application.Abstractions.Search;

using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Models;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Catalog.Queries.GetPaginatedProducts.V1;

internal sealed class GetPaginatedProductsQueryHandlerV1 : 
    IQueryHandler<GetPaginatedProductsQueryV1, PaginatedResult<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISearchService _searchService;
    private readonly ILogger<GetPaginatedProductsQueryHandlerV1> _logger;

    public GetPaginatedProductsQueryHandlerV1(
        IUnitOfWork unitOfWork,
        ISearchService searchService,
        ILogger<GetPaginatedProductsQueryHandlerV1> logger)
    {
        _unitOfWork = unitOfWork;
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<ProductDto>>> Handle(
        GetPaginatedProductsQueryV1 request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if we need to use search service (when any search parameters are provided)
            var useSearchService = !string.IsNullOrEmpty(request.SearchQuery) ||
                                   request.AttributeFilters.Any() ||
                                   request.PriceMin.HasValue ||
                                   request.PriceMax.HasValue ||
                                   request.CategoryIds.Any() ||
                                   request.InStockOnly;

            if (useSearchService)
            {
                var productListingRequest = new ProductListingRequest
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    SortColumn = MapSortColumn(request.SortColumn),
                    SortDescending = request.SortDescending,
                    CategoryId = request.CategoryId,
                    IsActiveOnly = request.IsActiveOnly,
                    SearchQuery = request.SearchQuery,
                    AttributeFilters = request.AttributeFilters,
                    PriceMin = request.PriceMin,
                    PriceMax = request.PriceMax,
                    CategoryIds = request.CategoryIds,
                    InStockOnly = request.InStockOnly
                };

                var searchResult = await _searchService.GetProductsAsync(productListingRequest, cancellationToken);
                if (searchResult.IsFailure)
                {
                    _logger.LogWarning("Search service failed, falling back to database query: {Error}", searchResult.Error.Message);
                    return await HandleDatabaseQuery(request, cancellationToken);
                }

                _logger.LogInformation(
                    "Retrieved products via search: Page {PageNumber}, Size {PageSize}, Total {TotalCount}", 
                    request.PageNumber, request.PageSize, searchResult.Value.TotalCount);

                return Result.Success(searchResult.Value);
            }

            // Fall back to database query for simple requests
            return await HandleDatabaseQuery(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paginated products");
            
            return Result.Failure<PaginatedResult<ProductDto>>(
                Error.Failure(
                    code: "Products.GetPaginatedFailed",
                    message: $"Failed to retrieve paginated products: {ex.Message}"));
        }
    }

    private async Task<Result<PaginatedResult<ProductDto>>> HandleDatabaseQuery(
        GetPaginatedProductsQueryV1 request, 
        CancellationToken cancellationToken)
    {
        PaginatedResult<ProductDto> paginatedResult;

        // If filtering by category, use the category-specific method
        if (request.CategoryId.HasValue)
        {
            paginatedResult = await _unitOfWork.ProductReader.GetPaginatedByCategoryAsync(
                request.CategoryId.Value,
                request.PageNumber,
                request.PageSize,
                MapSortColumn(request.SortColumn),
                request.SortDescending,
                cancellationToken);
        }
        else
        {
            // Use the general paginated method
            paginatedResult = await _unitOfWork.ProductReader.GetPaginatedAsync(
                request.PageNumber,
                request.PageSize,
                MapSortColumn(request.SortColumn),
                request.SortDescending,
                cancellationToken);
        }

        // Filter active products if requested (this should ideally be done at the repository level)
        if (request.IsActiveOnly)
        {
            var activeProducts = paginatedResult.Items.Where(p => p.IsActive).ToList();
            paginatedResult = PaginatedResult<ProductDto>.Create(
                activeProducts, 
                activeProducts.Count, 
                request.PageNumber, 
                request.PageSize);
        }
        
        _logger.LogInformation(
            "Retrieved paginated products from database: Page {PageNumber}, Size {PageSize}, Total {TotalCount}, CategoryId {CategoryId}", 
            paginatedResult.PageNumber, 
            paginatedResult.PageSize, 
            paginatedResult.TotalCount,
            request.CategoryId);
            
        return Result.Success(paginatedResult);
    }

    private static string MapSortColumn(string userFriendlyColumn)
    {
        return userFriendlyColumn.ToLower() switch
        {
            "name" => "Name",
            "price" => "BasePrice",
            "created" => "CreatedAt",
            "updated" => "UpdatedAt",
            _ => "Name" // Default fallback
        };
    }
}