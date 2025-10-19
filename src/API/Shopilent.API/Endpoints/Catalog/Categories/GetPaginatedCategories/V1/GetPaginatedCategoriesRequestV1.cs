namespace Shopilent.API.Endpoints.Catalog.Categories.GetPaginatedCategories.V1;

public class GetPaginatedCategoriesRequestV1
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string SortColumn { get; init; } = "Name";
    public bool SortDescending { get; init; } = false;
}