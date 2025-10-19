namespace Shopilent.Domain.Catalog.DTOs;

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public IReadOnlyList<VariantAttributeDto> Attributes { get; set; }
    public IReadOnlyList<ProductImageDto> Images { get; set; } // Added variant images
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}