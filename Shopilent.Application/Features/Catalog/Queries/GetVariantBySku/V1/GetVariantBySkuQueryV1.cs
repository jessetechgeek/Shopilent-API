using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.Application.Features.Catalog.Queries.GetVariantBySku.V1;

public sealed record GetVariantBySkuQueryV1 : IQuery<ProductVariantDto>, ICachedQuery<ProductVariantDto>
{
    public string Sku { get; init; } = string.Empty;

    public string CacheKey => $"variant-sku-{Sku}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
}