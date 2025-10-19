using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Sales.DTOs;

namespace Shopilent.Application.Features.Sales.Queries.GetUserOrders.V1;

public sealed record GetUserOrdersQueryV1 : IQuery<IReadOnlyList<OrderDto>>, ICachedQuery<IReadOnlyList<OrderDto>>
{
    public Guid UserId { get; init; }
    
    public string CacheKey => $"user-orders-{UserId}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15); // Shorter cache for orders as they change more frequently
}