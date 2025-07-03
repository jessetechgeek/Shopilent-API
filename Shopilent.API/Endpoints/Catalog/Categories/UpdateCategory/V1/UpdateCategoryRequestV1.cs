namespace Shopilent.API.Endpoints.Catalog.Categories.UpdateCategory.V1;

public class UpdateCategoryRequestV1
{
    public string Name { get; init; }
    public string Slug { get; init; }
    public string Description { get; init; }
    public bool? IsActive { get; init; }
}