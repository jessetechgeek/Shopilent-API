using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.Application.Features.Catalog.Queries.GetProductVariants.V1;

public sealed record GetProductVariantsQueryV1 : IQuery<IReadOnlyList<ProductVariantDto>>, ICachedQuery<IReadOnlyList<ProductVariantDto>>
{
    public Guid ProductId { get; init; }

    public string CacheKey => $"product-{ProductId}-variants";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
}