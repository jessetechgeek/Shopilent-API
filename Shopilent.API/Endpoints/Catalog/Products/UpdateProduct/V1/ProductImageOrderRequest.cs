namespace Shopilent.API.Endpoints.Catalog.Products.UpdateProduct.V1;

public class ProductImageOrderRequest
{
    public string ImageKey { get; init; }
    public int DisplayOrder { get; init; }
    public bool? IsDefault { get; init; }
}