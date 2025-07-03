namespace Shopilent.Application.Features.Catalog.Commands.UpdateVariantStock.V1;

public sealed class UpdateVariantStockResponseV1
{
    public Guid Id { get; init; }
    public int StockQuantity { get; init; }
    public bool IsActive { get; init; }
    public DateTime UpdatedAt { get; init; }
}