namespace Shopilent.API.Endpoints.Catalog.Products.AddProductVariant.V1;

public class ProductImageRequest
{
    public IFormFile File { get; init; }
    public string AltText { get; init; }
    public bool IsDefault { get; init; }
    public int DisplayOrder { get; init; }
}