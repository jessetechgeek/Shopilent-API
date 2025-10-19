using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Sales.Commands.MarkOrderAsDelivered.V1;

public sealed record MarkOrderAsDeliveredCommandV1 : ICommand<MarkOrderAsDeliveredResponseV1>
{
    public Guid OrderId { get; init; }
}