namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Dtos.Catalog;

internal class ProductVariantDtoWithJson
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public string MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}