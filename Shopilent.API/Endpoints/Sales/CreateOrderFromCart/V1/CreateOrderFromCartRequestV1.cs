namespace Shopilent.API.Endpoints.Sales.CreateOrderFromCart.V1;

public class CreateOrderFromCartRequestV1
{
    public Guid? CartId { get; init; }
    public Guid ShippingAddressId { get; init; }
    public Guid? BillingAddressId { get; init; }
    public string? ShippingMethod { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}