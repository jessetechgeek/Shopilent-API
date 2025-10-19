using Shopilent.Application.Abstractions.Caching;
using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Payments.DTOs;

namespace Shopilent.Application.Features.Payments.Queries.GetPaymentMethod.V1;

public sealed record GetPaymentMethodQueryV1 : IQuery<PaymentMethodDto>, ICachedQuery<PaymentMethodDto>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string CacheKey => $"payment-method-{Id}-user-{UserId}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
}