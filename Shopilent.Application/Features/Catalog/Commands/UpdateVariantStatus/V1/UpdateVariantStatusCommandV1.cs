using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateVariantStatus.V1;

public sealed record UpdateVariantStatusCommandV1 : ICommand
{
    public Guid Id { get; init; }
    public bool IsActive { get; init; }
}