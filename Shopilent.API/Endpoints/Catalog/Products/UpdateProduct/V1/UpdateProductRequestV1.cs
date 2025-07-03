namespace Shopilent.API.Endpoints.Catalog.Products.UpdateProduct.V1;

public class UpdateProductRequestV1
{
    public string Name { get; init; }
    public string Description { get; init; }
    public decimal BasePrice { get; init; }
    public string Slug { get; init; }
    public string? Sku { get; init; }
    public bool? IsActive { get; init; }
    public List<Guid>? CategoryIds { get; init; }
    public List<ProductAttributeRequest>? Attributes { get; init; }
    public List<IFormFile>? File { get; init; }
    public bool? RemoveExistingImages { get; init; }
   
    public List<string>? ImagesToRemove { get; init; }
    
    public List<ProductImageOrderRequest>? ImageOrders { get; init; }


}