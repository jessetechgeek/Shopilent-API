namespace Shopilent.Application.Features.Catalog.Commands.CreateProduct.V1;

public sealed class ProductImageResponseDto
{
    public string Url { get; init; }
    public string AltText { get; init; }
    public bool IsDefault { get; init; }
    public int DisplayOrder { get; init; }
}