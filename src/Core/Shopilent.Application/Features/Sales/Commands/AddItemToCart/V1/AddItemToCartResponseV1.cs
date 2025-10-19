namespace Shopilent.Application.Features.Sales.Commands.AddItemToCart.V1;

public sealed class AddItemToCartResponseV1
{
    public Guid CartId { get; init; }
    public Guid CartItemId { get; init; }
    public Guid ProductId { get; init; }
    public Guid? VariantId { get; init; }
    public int Quantity { get; init; }
    public string Message { get; init; }
}