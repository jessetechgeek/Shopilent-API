using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.DeleteAttribute.V1;

public sealed record DeleteAttributeCommandV1 : ICommand
{
    public Guid Id { get; init; }
}