namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Dtos.Catalog;

internal class ProductDetailDtoWithJson
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal BasePrice { get; set; }
    public string Currency { get; set; }
    public string Sku { get; set; }
    public string Slug { get; set; }
    public bool IsActive { get; set; }
    public string MetadataJson { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}