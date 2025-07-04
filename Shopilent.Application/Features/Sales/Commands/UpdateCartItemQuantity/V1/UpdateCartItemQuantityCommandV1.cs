using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Sales.Commands.UpdateCartItemQuantity.V1;

public sealed record UpdateCartItemQuantityCommandV1 : ICommand<UpdateCartItemQuantityResponseV1>
{
    public Guid CartItemId { get; init; }
    public int Quantity { get; init; }
}