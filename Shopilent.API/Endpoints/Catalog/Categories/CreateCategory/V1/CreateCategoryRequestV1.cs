namespace Shopilent.API.Endpoints.Catalog.Categories.CreateCategory.V1;

public class CreateCategoryRequestV1
{
    public string Name { get; init; }
    public string Slug { get; init; }
    public string Description { get; init; }
    public Guid? ParentId { get; init; }
}