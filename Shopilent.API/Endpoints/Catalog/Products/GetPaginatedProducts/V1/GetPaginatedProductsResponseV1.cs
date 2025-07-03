using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.API.Endpoints.Catalog.Products.GetPaginatedProducts.V1;

public class GetPaginatedProductsResponseV1
{
    public IReadOnlyList<ProductDto> Items { get; init; } = new List<ProductDto>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}