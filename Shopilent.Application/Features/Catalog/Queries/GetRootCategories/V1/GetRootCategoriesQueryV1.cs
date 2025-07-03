using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.Application.Features.Catalog.Queries.GetRootCategories.V1;

public sealed record GetRootCategoriesQueryV1 : IQuery<IReadOnlyList<CategoryDto>>, ICachedQuery<IReadOnlyList<CategoryDto>>
{
    public string CacheKey => "root-categories";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}