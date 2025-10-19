namespace Shopilent.Application.Abstractions.Search;

public class SearchResponse<T>
{
    public T[] Items { get; init; } = [];
    public SearchFacets Facets { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
    public string Query { get; init; } = "";
}

public class SearchFacets
{
    public CategoryFacet[] Categories { get; init; } = [];
    public AttributeFacet[] Attributes { get; init; } = [];
    public PriceRangeFacet PriceRange { get; init; } = new();
}

public class CategoryFacet
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public int Count { get; init; }
}

public class AttributeFacet
{
    public string Name { get; init; } = "";
    public AttributeValueFacet[] Values { get; init; } = [];
}

public class AttributeValueFacet
{
    public string Value { get; init; } = "";
    public int Count { get; init; }
}

public class PriceRangeFacet
{
    public decimal Min { get; init; }
    public decimal Max { get; init; }
}