using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.Application.Features.Catalog.Queries.GetAllAttributes.V1;

public sealed record GetAllAttributesQueryV1 : IQuery<IReadOnlyList<AttributeDto>>, ICachedQuery<IReadOnlyList<AttributeDto>>
{
    public string CacheKey => "all-attributes";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}