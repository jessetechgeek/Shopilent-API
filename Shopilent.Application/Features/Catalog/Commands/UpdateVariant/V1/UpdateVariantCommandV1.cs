using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateVariant.V1;

public sealed record UpdateVariantCommandV1 : ICommand<UpdateVariantResponseV1>
{
    public Guid Id { get; init; }
    public string? Sku { get; init; }
    public decimal? Price { get; init; }
    public int? StockQuantity { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
    public bool? IsActive { get; init; }
    public List<VariantImageDto>? Images { get; init; }
    public bool? RemoveExistingImages { get; init; }
    public List<string>? ImagesToRemove { get; init; }
    public List<VariantImageOrderDto>? ImageOrders { get; init; }
}

public sealed record VariantImageDto
{
    public Stream Url { get; init; }
    public string AltText { get; init; }
    public bool IsDefault { get; init; }
    public int DisplayOrder { get; init; }
}

public sealed record VariantImageOrderDto
{
    public string ImageKey { get; init; }
    public int DisplayOrder { get; init; }
    public bool? IsDefault { get; init; }
}