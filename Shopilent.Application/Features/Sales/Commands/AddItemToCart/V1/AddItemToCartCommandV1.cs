using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Sales.Commands.AddItemToCart.V1;

public sealed record AddItemToCartCommandV1 : ICommand<AddItemToCartResponseV1>
{
    public Guid? CartId { get; init; }
    public Guid ProductId { get; init; }
    public Guid? VariantId { get; init; }
    public int Quantity { get; init; } = 1;
}