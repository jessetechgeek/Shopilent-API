using Shopilent.Application.Abstractions.Messaging;
using System.Text.Json;

namespace Shopilent.Application.Features.Catalog.Commands.CreateProduct.V1;

public sealed record CreateProductCommandV1 : ICommand<CreateProductResponseV1>
{
    public string Name { get; init; }
    public string Slug { get; init; }
    public string Description { get; init; }
    public decimal BasePrice { get; init; }
    public string Currency { get; init; } = "USD";
    public string Sku { get; init; }
    public List<Guid> CategoryIds { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
    public bool IsActive { get; init; } = true;
    public List<ProductAttributeDto> Attributes { get; init; } = new();
    public List<ProductImageDto> Images { get; init; } = new();
}