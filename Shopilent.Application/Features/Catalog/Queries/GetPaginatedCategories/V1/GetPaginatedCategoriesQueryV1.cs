using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Catalog.DTOs;
using Shopilent.Domain.Common.Models;

namespace Shopilent.Application.Features.Catalog.Queries.GetPaginatedCategories.V1;

public sealed record GetPaginatedCategoriesQueryV1 : 
    IQuery<PaginatedResult<CategoryDto>>, 
    ICachedQuery<PaginatedResult<CategoryDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string SortColumn { get; init; } = "Name";
    public bool SortDescending { get; init; } = false;
    public string CacheKey => $"categories-page-{PageNumber}-size-{PageSize}-sort-{SortColumn}-{SortDescending}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
}