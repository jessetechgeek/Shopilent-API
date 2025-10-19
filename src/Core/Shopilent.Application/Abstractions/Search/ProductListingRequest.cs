namespace Shopilent.Application.Abstractions.Search;

public class ProductListingRequest
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
    public bool InStockOnly { get; init; } = false;
}