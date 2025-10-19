namespace Shopilent.API.Endpoints.Sales.AddItemToCart.V1;

public class AddItemToCartRequestV1
{
    public Guid? CartId { get; init; }
    public Guid ProductId { get; init; }
    public Guid? VariantId { get; init; }
    public int Quantity { get; init; } = 1;
}