using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Sales.Commands.AssignCartToUser.V1;

public sealed record AssignCartToUserCommandV1 : ICommand
{
    public Guid CartId { get; init; }
}