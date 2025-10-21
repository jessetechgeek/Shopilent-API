namespace Shopilent.Domain.Catalog.DTOs;

public class ProductAttributeDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid AttributeId { get; set; }
    public string AttributeName { get; set; }
    public string AttributeDisplayName { get; set; }
    public bool IsVariant { get; set; }
    public Dictionary<string, object> Values { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}