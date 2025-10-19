namespace Shopilent.Application.Features.Catalog.Commands.UpdateProduct.V1;

public sealed record ProductImageDto
{
    public Stream Url { get; init; }
    public string AltText { get; init; }
    public bool IsDefault { get; init; }
    public int DisplayOrder { get; init; }
}