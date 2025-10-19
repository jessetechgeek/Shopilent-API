using Shopilent.Application.Features.Catalog.Commands.AddProductVariant.V1;

namespace Shopilent.API.Endpoints.Catalog.Products.AddProductVariant.V1;

public class AddProductVariantRequestV1
{
    public string? Sku { get; init; }
    public decimal? Price { get; init; }
    public int StockQuantity { get; init; } = 0;
    public List<ProductAttributeDto> Attributes { get; init; } = new();
    public bool IsActive { get; init; } = true;
    public Dictionary<string, object>? Metadata { get; init; }
    public List<ProductImageRequest> Images { get; init; } = new();

    public List<IFormFile> File { get; init; }
}