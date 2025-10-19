namespace Shopilent.Application.Abstractions.Search;

public class ProductSearchResultDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string SKU { get; init; } = "";
    public string Slug { get; init; } = "";
    public decimal BasePrice { get; init; }
    public ProductSearchCategory[] Categories { get; init; } = [];
    public ProductSearchAttribute[] Attributes { get; init; } = [];
    public ProductSearchVariant[] Variants { get; init; } = [];
    public ProductSearchImage[] Images { get; init; } = [];
    public string Status { get; init; } = "";
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public ProductSearchPriceRange PriceRange { get; init; } = new();
    public bool HasStock { get; init; }
    public int TotalStock { get; init; }
}