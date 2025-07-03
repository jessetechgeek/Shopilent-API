using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateCategoryStatus.V1;

public sealed record UpdateCategoryStatusCommandV1 : ICommand
{
    public Guid Id { get; init; }
    public bool IsActive { get; init; }
}