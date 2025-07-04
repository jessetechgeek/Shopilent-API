using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Sales.Commands.RemoveItemFromCart.V1;

public sealed record RemoveItemFromCartCommandV1 : ICommand
{
    public Guid ItemId { get; init; }
}