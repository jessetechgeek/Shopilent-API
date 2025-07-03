using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.Application.Features.Catalog.Queries.GetChildCategories.V1;

public sealed record GetChildCategoriesQueryV1 : IQuery<IReadOnlyList<CategoryDto>>,
    ICachedQuery<IReadOnlyList<CategoryDto>>
{
    public Guid ParentId { get; init; }

    public string CacheKey => $"child-categories-{ParentId}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}