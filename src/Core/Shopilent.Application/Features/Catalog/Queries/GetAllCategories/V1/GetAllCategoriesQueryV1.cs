using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.Application.Features.Catalog.Queries.GetAllCategories.V1;

public sealed record GetAllCategoriesQueryV1 : IQuery<IReadOnlyList<CategoryDto>>, ICachedQuery<IReadOnlyList<CategoryDto>>
{
    public string CacheKey => "all-categories";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}