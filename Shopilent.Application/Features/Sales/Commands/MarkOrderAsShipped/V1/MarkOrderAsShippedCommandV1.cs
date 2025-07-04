using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Sales.Commands.MarkOrderAsShipped.V1;

public sealed record MarkOrderAsShippedCommandV1 : ICommand
{
    public Guid OrderId { get; init; }
    public string? TrackingNumber { get; init; }
}