using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Sales.DTOs;

namespace Shopilent.Application.Features.Sales.Queries.GetOrderDetails.V1;

public sealed record GetOrderDetailsQueryV1 : IQuery<OrderDetailDto>, ICachedQuery<OrderDetailDto>
{
    public Guid OrderId { get; init; }
    public Guid? CurrentUserId { get; init; }
    public bool IsAdmin { get; init; }
    public bool IsManager { get; init; }

    public string CacheKey => $"order-details-{OrderId}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15); // Shorter cache for order details
}