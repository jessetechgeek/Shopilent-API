namespace Shopilent.Application.Features.Catalog.Commands.UpdateProduct.V1;

public sealed record ProductImageOrderDto
{
    public string ImageKey { get; init; }
    public int DisplayOrder { get; init; }
    public bool? IsDefault { get; init; }
}