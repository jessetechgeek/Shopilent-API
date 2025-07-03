namespace Shopilent.API.Endpoints.Catalog.Attributes.UpdateAttribute.V1;

public class UpdateAttributeRequestV1
{
    public string DisplayName { get; init; }
    public bool Filterable { get; init; }
    public bool Searchable { get; init; }
    public bool IsVariant { get; init; }
    public Dictionary<string, object> Configuration { get; init; }
}