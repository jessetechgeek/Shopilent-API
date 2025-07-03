using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateVariantStock.V1;

public sealed record UpdateVariantStockCommandV1 : ICommand<UpdateVariantStockResponseV1>
{
    public Guid Id { get; init; }
    public int StockQuantity { get; init; }
}