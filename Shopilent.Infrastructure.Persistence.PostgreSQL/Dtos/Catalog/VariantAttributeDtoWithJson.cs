namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Dtos.Catalog;

internal class VariantAttributeDtoWithJson
{
    public Guid VariantId { get; set; }
    public Guid AttributeId { get; set; }
    public string AttributeName { get; set; }
    public string AttributeDisplayName { get; set; }
    public string ValueJson { get; set; }
}