using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.Application.Features.Catalog.Queries.GetVariant.V1;

public sealed record GetVariantQueryV1 : IQuery<ProductVariantDto>, ICachedQuery<ProductVariantDto>
{
    public Guid Id { get; init; }

    public string CacheKey => $"variant-{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
}