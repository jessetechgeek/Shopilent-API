using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateCategory.V1;

public sealed record UpdateCategoryCommandV1 : ICommand<UpdateCategoryResponseV1>
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Slug { get; init; }
    public string Description { get; init; }
    public bool? IsActive { get; init; }
}