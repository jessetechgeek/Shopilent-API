namespace Shopilent.API.Endpoints.Catalog.Categories.UpdateCategoryParent.V1;

public class UpdateCategoryParentResponseV1
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Slug { get; init; }
    public Guid? ParentId { get; init; }
    public string ParentName { get; init; }
    public int Level { get; init; }
    public string Path { get; init; }
    public DateTime UpdatedAt { get; init; }
}