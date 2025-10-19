namespace Shopilent.Domain.Catalog.DTOs;

public class ProductDetailDto : ProductDto
{
    public IReadOnlyList<CategoryDto> Categories { get; set; }
    public IReadOnlyList<ProductAttributeDto> Attributes { get; set; }
    public IReadOnlyList<ProductVariantDto> Variants { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? LastModified { get; set; }
}