namespace Shopilent.Application.Features.Catalog.Commands.AddProductVariant.V1;

public sealed class AddProductVariantResponseV1
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string? Sku { get; init; }
    public decimal? Price { get; init; }
    public int StockQuantity { get; init; }
    public bool IsActive { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<VariantAttributeDto> Attributes { get; init; } = new();
    public List<ProductImageResponseDto> Images { get; init; } = new();
}

public sealed class ProductImageResponseDto
{
    public string Url { get; init; }
    public string AltText { get; init; }
    public bool IsDefault { get; init; }
    public int DisplayOrder { get; init; }
}