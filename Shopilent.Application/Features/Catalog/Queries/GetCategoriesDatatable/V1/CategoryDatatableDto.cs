namespace Shopilent.Application.Features.Catalog.Queries.GetCategoriesDatatable.V1;

public sealed class CategoryDatatableDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public Guid? ParentId { get; set; }
    public string ParentName { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}