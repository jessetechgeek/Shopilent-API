using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateProductStatus.V1;

public sealed record UpdateProductStatusCommandV1 : ICommand
{
    public Guid Id { get; init; }
    public bool IsActive { get; init; }
}