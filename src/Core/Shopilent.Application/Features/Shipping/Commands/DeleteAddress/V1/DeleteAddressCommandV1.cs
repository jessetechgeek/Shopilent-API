using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Shipping.Commands.DeleteAddress.V1;

public sealed record DeleteAddressCommandV1 : ICommand
{
    public Guid Id { get; init; }
}