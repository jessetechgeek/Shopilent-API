namespace Shopilent.API.Endpoints.Catalog.Products.UpdateProduct.V1;

public class ProductAttributeRequest
{
    public Guid AttributeId { get; init; }
    public object Value { get; init; }
}