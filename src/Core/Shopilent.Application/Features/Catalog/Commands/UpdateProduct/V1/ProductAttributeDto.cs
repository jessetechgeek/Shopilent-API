namespace Shopilent.Application.Features.Catalog.Commands.UpdateProduct.V1;

public sealed record ProductAttributeDto
{
    public Guid AttributeId { get; init; }
    public object Value { get; init; }
}