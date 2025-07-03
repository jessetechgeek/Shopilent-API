namespace Shopilent.Application.Features.Catalog.Commands.AddProductVariant.V1;

public class VariantAttributeDto
{
    public Guid AttributeId { get; init; }
    public string Name { get; init; }
    public object Value { get; init; }
}