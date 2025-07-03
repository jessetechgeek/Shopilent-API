namespace Shopilent.Application.Features.Catalog.Commands.CreateProduct.V1;

public sealed class CreateProductResponseV1
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Slug { get; init; }
    public string Description { get; init; }
    public decimal BasePrice { get; init; }
    public string Currency { get; init; }
    public string Sku { get; init; }
    public bool IsActive { get; init; }
    public Dictionary<string, object> Metadata { get; init; }
    public List<Guid> CategoryIds { get; init; }
    public List<ProductAttributeResponseDto> Attributes { get; init; }
    public List<ProductImageResponseDto> Images { get; init; }
    public DateTime CreatedAt { get; init; }
}