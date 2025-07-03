namespace Shopilent.Application.Features.Catalog.Queries.GetAttributesDatatable.V1;

public sealed class AttributeDatatableDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Type { get; set; }
    public bool Filterable { get; set; }
    public bool Searchable { get; set; }
    public bool IsVariant { get; set; }
    public Dictionary<string, object> Configuration { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}