using System.Text.Json.Serialization;

namespace Shopilent.API.Common.Models;

public class ProductFilters
{
    [JsonPropertyName("attributeFilters")]
    public Dictionary<string, string[]> AttributeFilters { get; init; } = new();

    [JsonPropertyName("categorySlugs")]
    public string[] CategorySlugs { get; init; } = [];

    [JsonPropertyName("priceMin")]
    public decimal? PriceMin { get; init; }

    [JsonPropertyName("priceMax")]
    public decimal? PriceMax { get; init; }

    [JsonPropertyName("inStockOnly")]
    public bool InStockOnly { get; init; } = false;

    [JsonPropertyName("activeOnly")]
    public bool ActiveOnly { get; init; } = true;

    [JsonPropertyName("searchQuery")]
    public string SearchQuery { get; init; } = "";

    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; init; } = 1;

    [JsonPropertyName("pageSize")]
    public int PageSize { get; init; } = 20;

    [JsonPropertyName("sortBy")]
    public string SortBy { get; init; } = "name";

    [JsonPropertyName("sortDescending")]
    public bool SortDescending { get; init; } = false;

    [JsonPropertyName("sortColumn")]
    public string SortColumn { get; init; } = "Name";
}