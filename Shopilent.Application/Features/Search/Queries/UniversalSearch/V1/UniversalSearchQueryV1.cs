using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Application.Abstractions.Search;


namespace Shopilent.Application.Features.Search.Queries.UniversalSearch.V1;

public record UniversalSearchQueryV1(
    string Query = "",
    Guid[] CategoryIds = default!,
    Dictionary<string, string[]> AttributeFilters = default!,
    decimal? PriceMin = null,
    decimal? PriceMax = null,
    bool InStockOnly = false,
    bool ActiveOnly = true,
    int PageNumber = 1,
    int PageSize = 20,
    string SortBy = "relevance",
    bool SortDescending = false
) : IQuery<SearchResponse<ProductSearchResultDto>>
{
    public UniversalSearchQueryV1() : this("", Array.Empty<Guid>(), new Dictionary<string, string[]>()) { }
}