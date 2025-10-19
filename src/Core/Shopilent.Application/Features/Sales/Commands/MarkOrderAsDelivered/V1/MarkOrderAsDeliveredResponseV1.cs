using Shopilent.Domain.Sales.Enums;

namespace Shopilent.Application.Features.Sales.Commands.MarkOrderAsDelivered.V1;

public sealed class MarkOrderAsDeliveredResponseV1
{
    public Guid Id { get; init; }
    public OrderStatus Status { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string Message { get; init; }
}