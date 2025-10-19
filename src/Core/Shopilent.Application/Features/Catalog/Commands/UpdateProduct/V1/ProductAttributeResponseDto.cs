namespace Shopilent.Application.Features.Catalog.Commands.UpdateProduct.V1;

public sealed class ProductAttributeResponseDto
{
    public Guid AttributeId { get; init; }
    public string AttributeName { get; init; }
    public object Values { get; init; }
}