using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Payments.DTOs;

namespace Shopilent.Application.Features.Payments.Queries.GetUserPaymentMethods.V1;

public sealed record GetUserPaymentMethodsQueryV1 : IQuery<IReadOnlyList<PaymentMethodDto>>
{
    public Guid UserId { get; init; }
}