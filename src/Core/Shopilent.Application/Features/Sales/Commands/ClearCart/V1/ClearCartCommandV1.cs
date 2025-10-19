using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Sales.Commands.ClearCart.V1;

public sealed record ClearCartCommandV1 : ICommand
{
    public Guid? CartId { get; init; }
}