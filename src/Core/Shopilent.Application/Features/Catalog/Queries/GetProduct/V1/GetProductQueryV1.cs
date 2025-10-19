using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Catalog.DTOs;

namespace Shopilent.Application.Features.Catalog.Queries.GetProduct.V1;

public sealed record GetProductQueryV1 : IQuery<ProductDetailDto>, ICachedQuery<ProductDetailDto>
{
    public Guid Id { get; init; }

    public string CacheKey => $"product-{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}