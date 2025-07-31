using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Models;

namespace Shopilent.Application.Features.Catalog.Queries.GetPaginatedProducts.V1;

public sealed record GetPaginatedProductsQueryV1 :
    IQuery<PaginatedResult<ProductDto>>,
    ICachedQuery<PaginatedResult<ProductDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string SortColumn { get; init; } = "Name";
    public bool SortDescending { get; init; } = false;
    public Guid? CategoryId { get; init; }
    public bool IsActiveOnly { get; init; } = true;
    
    public string SearchQuery { get; init; } = "";
    public Dictionary<string, string[]> AttributeFilters { get; init; } = new();
    public decimal? PriceMin { get; init; }
    public decimal? PriceMax { get; init; }
    public Guid[] CategoryIds { get; init; } = [];
    public bool InStockOnly { get; init; } = false;

    public string CacheKey =>
        $"products-page-{PageNumber}-size-{PageSize}-sort-{SortColumn}-{SortDescending}-category-{CategoryId}-active-{IsActiveOnly}-search-{SearchQuery.GetHashCode()}-filters-{GetAttributeFiltersHash()}-price-{PriceMin}-{PriceMax}-categories-{string.Join(",", CategoryIds)}-stock-{InStockOnly}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(15); // Products change more frequently than categories
    
    private int GetAttributeFiltersHash()
    {
        if (!AttributeFilters.Any()) return 0;
        
        var hash = 17;
        foreach (var (key, values) in AttributeFilters.OrderBy(x => x.Key))
        {
            hash = hash * 23 + key.GetHashCode();
            hash = hash * 23 + string.Join(",", values.OrderBy(x => x)).GetHashCode();
        }
        return hash;
    }
}