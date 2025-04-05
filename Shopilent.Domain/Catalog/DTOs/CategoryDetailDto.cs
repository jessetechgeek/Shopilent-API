namespace Shopilent.Domain.Catalog.DTOs;

public class CategoryDetailDto : CategoryDto
{
    public string ParentName { get; set; }
    public int ProductCount { get; set; }
}