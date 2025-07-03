using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.Application.Features.Catalog.Queries.GetAttribute.V1;

public sealed record GetAttributeQueryV1 : IQuery<AttributeDto>, ICachedQuery<AttributeDto>
{
    public Guid Id { get; init; }

    public string CacheKey => $"attribute-{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}