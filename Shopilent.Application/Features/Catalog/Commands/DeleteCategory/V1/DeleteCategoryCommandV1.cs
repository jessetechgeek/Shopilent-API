using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.DeleteCategory.V1;

public sealed record DeleteCategoryCommandV1 : ICommand
{
    public Guid Id { get; init; }
}