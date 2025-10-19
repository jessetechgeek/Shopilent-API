namespace Shopilent.Application.Features.Catalog.Commands.UpdateProduct.V1;

public sealed class UpdateProductResponseV1
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public decimal BasePrice { get; init; }
    public string Currency { get; init; }
    public string Slug { get; init; }
    public string? Sku { get; init; }
    public bool IsActive { get; init; }
    public List<Guid> CategoryIds { get; init; }
    public List<ProductAttributeResponseDto> Attributes { get; init; }
    public List<ProductImageResponseDto> Images { get; init; }
    public DateTime UpdatedAt { get; init; }
}