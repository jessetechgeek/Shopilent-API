using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.Application.Features.Catalog.Queries.GetCategory.V1;

public sealed record GetCategoryQueryV1 : IQuery<CategoryDto>, ICachedQuery<CategoryDto>
{
    public Guid Id { get; init; }

    public string CacheKey => $"category-{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}