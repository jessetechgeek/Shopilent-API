using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Catalog.Commands.UpdateCategoryParent.V1;


public sealed record UpdateCategoryParentCommandV1 : ICommand<UpdateCategoryParentResponseV1>
{
    public Guid Id { get; init; }
    public Guid? ParentId { get; init; }
}