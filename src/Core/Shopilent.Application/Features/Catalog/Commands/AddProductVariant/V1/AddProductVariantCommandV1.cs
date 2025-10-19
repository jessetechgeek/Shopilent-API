using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.AddProductVariant.V1;

public sealed record AddProductVariantCommandV1 : ICommand<AddProductVariantResponseV1>
{
    public Guid ProductId { get; init; }
    public string? Sku { get; init; }
    public decimal? Price { get; init; }
    public int StockQuantity { get; init; }
    public List<ProductAttributeDto> Attributes { get; init; } = new();
    public bool IsActive { get; init; } = true;
    public Dictionary<string, object>? Metadata { get; init; }
    public List<ProductImageDto> Images { get; init; } = new();
}

public sealed record ProductAttributeDto
{
    public Guid AttributeId { get; init; }
    public object Value { get; init; }
}

public sealed record ProductImageDto
{
    public Stream Url { get; init; }
    public string AltText { get; init; }
    public bool IsDefault { get; init; }
    public int DisplayOrder { get; init; }
}