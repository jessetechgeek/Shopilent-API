using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Payments.Commands.DeletePaymentMethod.V1;


public sealed record DeletePaymentMethodCommandV1 : ICommand
{
    public Guid Id { get; init; }
}