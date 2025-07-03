using Shopilent.Domain.Catalog.Enums;

namespace Shopilent.API.Endpoints.Catalog.Attributes.UpdateAttribute.V1;

public class UpdateAttributeResponseV1
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string DisplayName { get; init; }
    public AttributeType Type { get; init; }
    public bool Filterable { get; init; }
    public bool Searchable { get; init; }
    public bool IsVariant { get; init; }
    public Dictionary<string, object> Configuration { get; init; }
    public DateTime UpdatedAt { get; init; }
}