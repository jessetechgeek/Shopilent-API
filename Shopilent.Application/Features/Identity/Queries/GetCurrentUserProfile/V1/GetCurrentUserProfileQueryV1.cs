using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Identity.DTOs;

namespace Shopilent.Application.Features.Identity.Queries.GetCurrentUserProfile.V1;

public sealed record GetCurrentUserProfileQueryV1 : IQuery<UserDetailDto>, ICachedQuery<UserDetailDto>
{
    public string CacheKey => $"user-profile-{UserId}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
    public Guid UserId { get; init; }
}