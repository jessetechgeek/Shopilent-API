namespace Shopilent.Application.Features.Sales.Commands.UpdateCartItemQuantity.V1;

public sealed class UpdateCartItemQuantityResponseV1
{
    public Guid CartItemId { get; init; }
    public int Quantity { get; init; }
    public DateTime UpdatedAt { get; init; }
}