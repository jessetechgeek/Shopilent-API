using Shopilent.Domain.Catalog.Enums;

namespace Shopilent.Domain.Catalog.DTOs;

public class AttributeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public AttributeType Type { get; set; }
    public Dictionary<string, object> Configuration { get; set; }
    public bool Filterable { get; set; }
    public bool Searchable { get; set; }
    public bool IsVariant { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}