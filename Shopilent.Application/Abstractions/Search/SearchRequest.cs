namespace Shopilent.Application.Abstractions.Search;

public class SearchRequest
{
    public string Query { get; init; } = "";
    public Guid[] CategoryIds { get; init; } = [];
    public Dictionary<string, string[]> AttributeFilters { get; init; } = new();
    public decimal? PriceMin { get; init; }
    public decimal? PriceMax { get; init; }
    public bool InStockOnly { get; init; } = false;
    public bool ActiveOnly { get; init; } = true;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "relevance";
    public bool SortDescending { get; init; } = false;
}