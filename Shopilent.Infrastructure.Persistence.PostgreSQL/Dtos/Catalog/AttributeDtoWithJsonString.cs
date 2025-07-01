namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Repositories.Catalog.Read;

internal class AttributeDtoWithJsonString
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Type { get; set; }
    public string ConfigurationJson { get; set; }
    public bool Filterable { get; set; }
    public bool Searchable { get; set; }
    public bool IsVariant { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}