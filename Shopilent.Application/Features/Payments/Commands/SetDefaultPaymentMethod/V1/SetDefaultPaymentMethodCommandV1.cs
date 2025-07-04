using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Payments.Commands.SetDefaultPaymentMethod.V1;

public sealed record SetDefaultPaymentMethodCommandV1 : ICommand<SetDefaultPaymentMethodResponseV1>
{
    public Guid PaymentMethodId { get; init; }
    public Guid UserId { get; init; }
}