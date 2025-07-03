using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Identity.DTOs;

namespace Shopilent.Application.Features.Identity.Queries.GetUser.V1;

public sealed record GetUserQueryV1 : IQuery<UserDetailDto>, ICachedQuery<UserDetailDto>
{
    public Guid Id { get; init; }

    public string CacheKey => $"user-detail-{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
}