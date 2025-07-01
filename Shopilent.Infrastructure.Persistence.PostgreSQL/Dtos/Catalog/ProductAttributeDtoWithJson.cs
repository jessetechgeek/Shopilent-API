namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Dtos.Catalog;

internal class ProductAttributeDtoWithJson
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid AttributeId { get; set; }
    public string AttributeName { get; set; }
    public string AttributeDisplayName { get; set; }
    public string ValuesJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}