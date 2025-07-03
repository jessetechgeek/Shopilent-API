namespace Shopilent.Application.Features.Catalog.Commands.CreateProduct.V1;

public sealed record ProductAttributeDto
{
    public Guid AttributeId { get; init; }
    public object Value { get; init; }
}