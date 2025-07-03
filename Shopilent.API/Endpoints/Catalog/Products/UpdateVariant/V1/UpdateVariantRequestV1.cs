namespace Shopilent.API.Endpoints.Catalog.Products.UpdateVariant.V1;

public class UpdateVariantRequestV1
{
    public string? Sku { get; init; }
    public decimal? Price { get; init; }
    public int? StockQuantity { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
    public bool? IsActive { get; init; }
    public List<IFormFile>? File { get; init; }
    public bool? RemoveExistingImages { get; init; }
    public List<string>? ImagesToRemove { get; init; }
    public List<VariantImageOrderRequest>? ImageOrders { get; init; }
}

public class VariantImageOrderRequest
{
    public string ImageKey { get; init; }
    public int DisplayOrder { get; init; }
    public bool? IsDefault { get; init; }
}