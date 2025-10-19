using Shopilent.Domain.Catalog.ValueObjects;

namespace Shopilent.Domain.Catalog.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal BasePrice { get; set; }
    public string Currency { get; set; }
    public string Sku { get; set; }
    public string Slug { get; set; }
    public bool IsActive { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public IReadOnlyList<ProductImageDto> Images { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}