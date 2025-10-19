using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.DeleteProduct.V1;

public sealed record DeleteProductCommandV1 : ICommand
{
    public Guid Id { get; init; }
}