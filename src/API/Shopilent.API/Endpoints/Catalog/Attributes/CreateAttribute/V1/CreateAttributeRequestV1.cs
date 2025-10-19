namespace Shopilent.API.Endpoints.Catalog.Attributes.CreateAttribute.V1;

public class CreateAttributeRequestV1
{
    public string Name { get; init; }
    public string DisplayName { get; init; }
    public string Type { get; init; }

    public bool Filterable { get; init; }
    public bool Searchable { get; init; }
    public bool IsVariant { get; init; }
    public Dictionary<string, object> Configuration { get; init; }
}