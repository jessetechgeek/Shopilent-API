namespace Shopilent.Domain.Catalog.DTOs;

public class VariantAttributeDto
{
    public Guid VariantId { get; set; }
    public Guid AttributeId { get; set; }
    public string AttributeName { get; set; }
    public string AttributeDisplayName { get; set; }
    public Dictionary<string, object> Value { get; set; }
}