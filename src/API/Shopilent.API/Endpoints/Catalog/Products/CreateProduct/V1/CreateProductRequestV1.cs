namespace Shopilent.API.Endpoints.Catalog.Products.CreateProduct.V1;

public class CreateProductRequestV1
{
    public string Name { get; init; }
    public string Slug { get; init; }
    public string Description { get; init; }
    public decimal BasePrice { get; init; }
    public string Currency { get; init; } = "USD";
    public string Sku { get; init; }
    public List<Guid> CategoryIds { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
    public bool IsActive { get; init; } = true;
    public List<ProductAttributeRequest> Attributes { get; init; } = new();
    public List<ProductImageRequest> Images { get; init; } = new();
    
    public List<IFormFile> File { get; init; }
}