using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Shipping.DTOs;

namespace Shopilent.Application.Features.Shipping.Queries.GetUserAddresses.V1;

public sealed record GetUserAddressesQueryV1 : IQuery<IReadOnlyList<AddressDto>>,
    ICachedQuery<IReadOnlyList<AddressDto>>
{
    public Guid UserId { get; init; }

    public string CacheKey => $"user-addresses-{UserId}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
}