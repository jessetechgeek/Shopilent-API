using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.API.Endpoints.Catalog.Categories.GetPaginatedCategories.V1;

public class GetPaginatedCategoriesResponseV1
{
    public IReadOnlyList<CategoryDto> Items { get; init; } = new List<CategoryDto>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}