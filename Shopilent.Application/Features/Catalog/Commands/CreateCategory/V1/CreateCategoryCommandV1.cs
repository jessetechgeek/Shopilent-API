using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.CreateCategory.V1;

public sealed record CreateCategoryCommandV1 : ICommand<CreateCategoryResponseV1>
{
    public string Name { get; init; }
    public string Slug { get; init; }
    public string Description { get; init; }
    public Guid? ParentId { get; init; }
}