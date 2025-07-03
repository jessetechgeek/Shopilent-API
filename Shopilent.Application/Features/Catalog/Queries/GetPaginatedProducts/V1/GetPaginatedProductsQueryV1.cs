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

    public string CacheKey =>
        $"products-page-{PageNumber}-size-{PageSize}-sort-{SortColumn}-{SortDescending}-category-{CategoryId}-active-{IsActiveOnly}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(15); // Products change more frequently than categories
}