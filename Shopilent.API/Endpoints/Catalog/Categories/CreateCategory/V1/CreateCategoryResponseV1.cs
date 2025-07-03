namespace Shopilent.API.Endpoints.Catalog.Categories.CreateCategory.V1;

public class CreateCategoryResponseV1
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Slug { get; init; }
    public string Description { get; init; }
    public Guid? ParentId { get; init; }
    public int Level { get; init; }
    public string Path { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}