using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateProduct.V1;

public sealed record UpdateProductCommandV1 : ICommand<UpdateProductResponseV1>
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public decimal BasePrice { get; init; }
    public string Slug { get; init; }
    public string? Sku { get; init; }
    public bool? IsActive { get; init; }
    public List<Guid>? CategoryIds { get; init; }
    public List<ProductAttributeDto>? Attributes { get; init; }
    public List<ProductImageDto>? Images { get; init; }
    public bool? RemoveExistingImages { get; init; }
    public List<string>? ImagesToRemove { get; init; }
    public List<ProductImageOrderDto>? ImageOrders { get; init; }
}