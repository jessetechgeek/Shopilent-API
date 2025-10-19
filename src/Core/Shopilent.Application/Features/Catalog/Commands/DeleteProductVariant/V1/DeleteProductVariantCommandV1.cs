using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.DeleteProductVariant.V1;

public sealed record DeleteProductVariantCommandV1 : ICommand
{
    public Guid Id { get; init; }
}